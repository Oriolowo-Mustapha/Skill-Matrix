using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Skill_Matrix.Data;
using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using Skill_Matrix.ViewModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SkillMatrix.Services
{
	public class QuizService : IQuizService
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IQuizRepository _quizResultRepository;
		private readonly ISuggestionRepository _suggestionRepository;
		private readonly HttpClient _httpClient;
		private readonly string _geminiApiKey;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IMemoryCache _cache;
		private readonly SkillMatrixDbContext _dbContext;

		public QuizService(ISkillRepository skillRepository, IQuizRepository quizResultRepository, ISuggestionRepository suggestionRepository, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, SkillMatrixDbContext dbContext)
		{
			_skillRepository = skillRepository;
			_quizResultRepository = quizResultRepository;
			_suggestionRepository = suggestionRepository;
			_httpClient = httpClient;
			_geminiApiKey = configuration["Gemini:ApiKey"];
			_httpContextAccessor = httpContextAccessor;
			_cache = cache;
			_dbContext = dbContext;
		}

		public async Task<QuizBatchDto> GetQuizQuestionsAsync(string skillName, int count, string ProficiencyLevel)
		{
			var prompt = $"Generate {count} multiple-choice questions on the core concepts of {skillName}, " +
			 $"tailored to a {ProficiencyLevel} level. " +
			 $"Each question should have 4 options labeled A to D. " +
			 $"Include questions that test deep understanding, such as those related to algorithms, common data structures, or fundamental principles of {skillName}. " +
			 $"The questions should focus on the 'why' and 'how' rather than just basic syntax. " +
			 $"Include the correct answer. " +
			 $"Format the output as a JSON array of objects, where each object has 'question', 'options' (an array of strings), and 'correctAnswer' (the text of the correct option, not just the letter)." +
			 $"Return valid JSON only." +
			 $"Do not use markdown or trailing commas.";

			var requestBody = new
			{
				contents = new[]
				{
							new {
								parts = new[]
								{
									new { text = prompt }
								}
							}
						},
				generationConfig = new
				{
					temperature = 0.7,
					topP = 0.95,
					topK = 40,
					maxOutputTokens = 2048
				}
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

			using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}");
			request.Content = content;

			var response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				throw new Exception($"Failed to fetch quiz questions from Gemini. Status: {response.StatusCode}, Content: {errorContent}");
			}

			var json = await response.Content.ReadAsStringAsync();
			var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(json);
			var geminiContent = geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
			var quizQuestions = await ParseQuizFromJsonAsync(geminiContent ?? string.Empty);
			var userId = GetCurrentUserId();
			var skill = await _skillRepository.GetByUserIdandSkillNameAsync(userId, skillName);
			var batchId = await SaveParsedQuizAsync(userId, skill.Id, quizQuestions);
			_cache.Set($"QuizQuestions_{skill.Id}", quizQuestions, TimeSpan.FromMinutes(30));

			return new QuizBatchDto { BatchId = batchId, Questions = quizQuestions };
		}
		public async Task<QuizResultDto> SubmitQuizAsync(Guid userId, Guid skillId, int quizBatchId, List<string> answers)
		{
			var skill = await _skillRepository.GetByIdAsync(skillId);
			if (skill == null || skill.UserId != userId)
				throw new Exception("Invalid skill or user.");

			var quizQuestions = await _quizResultRepository.GetByBatchId(quizBatchId);
			if (quizQuestions == null || !quizQuestions.Any())
				throw new Exception("No quiz questions found for this batch.");

			if (quizQuestions.Count != answers.Count)
				throw new Exception("Number of answers does not match the number of questions.");

			int correctCount = 0;
			int wrongCount = 0;
			var quizResultQuestions = new List<QuizQuestions>();

			var quizBatch = new QuizBatch
			{
				UserId = userId,
				SkillId = skillId,
				CreatedAt = DateTime.UtcNow
			};

			for (int i = 0; i < quizQuestions.Count; i++)
			{
				var question = quizQuestions[i];
				var userAnswer = answers[i];

				bool isCorrect = question.CorrectAnswer == userAnswer;
				if (isCorrect)
				{
					correctCount++;
				}
				else
				{
					wrongCount++;

					question.WrongAnswers.Add(new WrongAnswers
					{
						AnswerText = userAnswer,
						QuizQuestionId = question.Id,
						QuizBatch = quizBatch
					});
				}

				// Attach this question to batch
				quizResultQuestions.Add(question);
			}

			quizBatch.Questions = quizResultQuestions;

			// 4. Calculate score
			int score = (int)((correctCount / (double)quizQuestions.Count) * 100);

			// 5. Determine proficiency level
			string proficiencyLevel = score switch
			{
				<= 40 => "Beginner",
				<= 70 => "Intermediate",
				<= 90 => "Advanced",
				_ => "Expert"
			};

			var previousResult = await _quizResultRepository.GetLatestByUserAndSkillAsync(userId, skillId);
			int retakeCount = previousResult != null ? previousResult.RetakeCount + 1 : 0;

			// 6. Create quiz result
			var quizResult = new QuizResult
			{
				UserId = userId,
				SkillId = skillId,
				QuizBatch = quizBatch,
				Score = score,
				ProficiencyLevel = proficiencyLevel,
				RetakeCount = retakeCount,
				NoOfCorrectAnswers = correctCount,
				NoOfWrongAnswers = wrongCount,
				DateTaken = DateTime.UtcNow
			};

			// 7. Update skill
			skill.ProficiencyLevel = proficiencyLevel;
			skill.LastAssessed = DateTime.UtcNow;

			// 8. Save all
			await _quizResultRepository.AddAsync(quizResult);
			await _skillRepository.UpdateAsync(skill);

			// 9. Return DTO
			return new QuizResultDto
			{
				Id = quizResult.Id,
				SkillId = skillId,
				SkillName = skill.SkillName,
				Score = score,
				ProficiencyLevel = proficiencyLevel,
				DateTaken = quizResult.DateTaken,
				NoOfCorrectAnswers = correctCount,
				NoOfWrongAnswers = wrongCount,
				RetakeCount = retakeCount
			};
		}
		public async Task<QuizBatchDto> CreateRetakeQuizAsync(Guid userId, Guid skillId)
		{
			var skill = await _skillRepository.GetByIdAsync(skillId);
			if (skill == null) throw new Exception("Skill not found.");

			var lastResult = await _quizResultRepository.GetLatestByUserAndSkillAsync(userId, skillId);

			string prompt;
			int totalQuestions = 10;

			if (lastResult == null)
			{
				return await GetQuizQuestionsAsync(skill.SkillName, totalQuestions, "Beginner");
			}

			var suggestions = await _suggestionRepository.GetSuggestionsForQuizResultAsync(lastResult.Id);
			var suggestedTopics = suggestions.Select(s => s.Suggestions).Distinct().ToList();
			var userProficiency = lastResult.ProficiencyLevel;
			var lastScore = lastResult.Score;

			StringBuilder promptBuilder = new StringBuilder();
			promptBuilder.Append($"Generate {totalQuestions} multiple-choice questions on the core concepts of {skill.SkillName}, ");
			promptBuilder.Append($"tailored to a {userProficiency} level. ");
			promptBuilder.Append($"The user's last score for this skill was {lastScore}%. ");

			if (suggestedTopics.Any())
			{
				promptBuilder.Append($"Focus heavily on the following areas where the user showed weakness: {string.Join(", ", suggestedTopics)}. ");
				promptBuilder.Append("Ensure a significant portion of the questions (e.g., 60-70%) cover these specific topics. ");
			}
			else
			{
				promptBuilder.Append("Since there are no specific weak areas identified, generate questions that cover a broad range of topics within this skill. ");
			}

			promptBuilder.Append("Each question should have 4 options labeled A to D. ");
			promptBuilder.Append("Include questions that test deep understanding, such as those related to algorithms, common data structures, or fundamental principles. ");
			promptBuilder.Append("The questions should focus on the 'why' and 'how' rather than just basic syntax. ");
			promptBuilder.Append("Include the correct answer. ");
			promptBuilder.Append("Format the output as a JSON array of objects, where each object has 'question', 'options' (an array of strings), and 'correctAnswer' (the text of the correct option, not just the letter).");
			promptBuilder.Append("Return valid JSON only. Do not use markdown or trailing commas.");

			prompt = promptBuilder.ToString();

			var requestBody = new
			{
				contents = new[]
				{
					new {
						parts = new[]
						{
							new { text = prompt }
						}
					}
				},
				generationConfig = new
				{
					temperature = 0.7,
					topP = 0.95,
					topK = 40,
					maxOutputTokens = 2048
				}
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

			using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}");
			request.Content = content;

			var response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				throw new Exception($"Failed to fetch retake quiz questions from Gemini. Status: {response.StatusCode}, Content: {errorContent}");
			}

			var json = await response.Content.ReadAsStringAsync();
			var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(json);

			var geminiContent = geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
			var quizQuestions = await ParseQuizFromJsonAsync(geminiContent ?? string.Empty);

			// Save the newly generated quiz to the database
			var batchId = await SaveParsedQuizAsync(userId, skillId, quizQuestions);

			// Cache the generated quiz
			_cache.Set($"QuizQuestions_{skillId}", quizQuestions, TimeSpan.FromMinutes(30));

			return new QuizBatchDto { BatchId = batchId, Questions = quizQuestions };
		}
		private async Task<List<QuizDto>> ParseQuizFromJsonAsync(string jsonText)
		{
			if (string.IsNullOrWhiteSpace(jsonText))
			{
				return new List<QuizDto>();
			}

			string cleanedJsonText = jsonText;

			// Attempt to find the start and end of the actual JSON content
			int startIndex = cleanedJsonText.IndexOfAny(new char[] { '[', '{' });
			int endIndex = cleanedJsonText.LastIndexOfAny(new char[] { ']', '}' });

			if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
			{
				cleanedJsonText = cleanedJsonText.Substring(startIndex, endIndex - startIndex + 1);
			}
			else if (cleanedJsonText.StartsWith("```json") && cleanedJsonText.EndsWith("```"))
			{
				// Remove markdown code block wrappers
				cleanedJsonText = cleanedJsonText.Substring("```json".Length).Trim();
				cleanedJsonText = cleanedJsonText.Substring(0, cleanedJsonText.Length - "```".Length).Trim();
			}

			// 🔹 Clean up trailing commas before ] or }
			cleanedJsonText = Regex.Replace(cleanedJsonText, @",(\s*[\]}])", "$1");

			try
			{
				var quizQuestions = JsonSerializer.Deserialize<List<QuizDto>>(
					cleanedJsonText,
					new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true,
						AllowTrailingCommas = true // ✅ tolerate small JSON mistakes
					});

				return quizQuestions ?? new List<QuizDto>();
			}
			catch (JsonException ex)
			{
				Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
				Console.WriteLine($"Original JSON: {jsonText}");
				Console.WriteLine($"Cleaned JSON Attempt: {cleanedJsonText}");
				throw new Exception("Failed to parse quiz questions from Gemini's JSON response.", ex);
			}
		}

		public async Task<int> SaveParsedQuizAsync(Guid userId, Guid skillId, List<QuizDto> quizDtos)
		{
			if (quizDtos == null || !quizDtos.Any())
				throw new Exception("No quiz questions to save.");

			var batch = new QuizBatch
			{
				SkillId = skillId,
				UserId = userId,
				CreatedAt = DateTime.UtcNow
			};

			foreach (var dto in quizDtos)
			{
				var quizQuestion = new QuizQuestions
				{
					Question = dto.Question,
					CorrectAnswer = dto.CorrectAnswer,
					Options = dto.Options.Select(opt => new Options
					{
						OptionText = opt
					}).ToList()
				};

				batch.Questions.Add(quizQuestion);
			}

			return await _quizResultRepository.AddQuestionAsync(batch);
		}


		public List<QuizDto> GetQuizQuestionsFromCache(Guid skillId)
		{
			var cacheKey = $"QuizQuestions_{skillId}";
			var userId = GetCurrentUserId();

			if (!_cache.TryGetValue(cacheKey, out List<QuizQuestions> quizQuestions))
			{
				// Cache miss → Load ONLY the latest batch from DB
				var latestBatch = _dbContext.QuizBatches
					.Where(b => b.SkillId == skillId && b.UserId == userId)
					.OrderByDescending(b => b.CreatedAt)
					.Include(b => b.Questions)
						.ThenInclude(q => q.Options)
					.FirstOrDefault();

				quizQuestions = latestBatch?.Questions ?? new List<QuizQuestions>();

				// Save into cache
				_cache.Set(cacheKey, quizQuestions, TimeSpan.FromMinutes(30));
			}

			// 🔹 Convert Entities → DTOs
			var quizDtos = quizQuestions.Select(q => new QuizDto
			{
				Question = q.Question,
				Options = q.Options.Select(opt => opt.OptionText).ToList(),
				CorrectAnswer = q.CorrectAnswer
			}).ToList();

			return quizDtos;
		}


		private Guid GetCurrentUserId()
		{
			var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
			if (string.IsNullOrEmpty(userIdString))
				throw new UnauthorizedAccessException("User not logged in.");

			return Guid.Parse(userIdString);
		}

		public async Task<List<AssessmentViewModel>> UsersAssesment(Guid userId)
		{
			return await _quizResultRepository.UserAssessments(userId);
		}
	}
}