using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace SkillMatrix.Services
{
	public class QuizService : IQuizService
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IQuizRepository _quizResultRepository;
		private readonly HttpClient _httpClient;
		private readonly string _geminiApiKey;

		public QuizService(ISkillRepository skillRepository, IQuizRepository quizResultRepository, HttpClient httpClient, IConfiguration configuration)
		{
			_skillRepository = skillRepository;
			_quizResultRepository = quizResultRepository;
			_httpClient = httpClient;
			_geminiApiKey = configuration["Gemini:ApiKey"]; // Changed to Gemini API key from appsettings.json
		}
		public List<QuizDto> quizQuestionsForScoring;

		public async Task<List<QuizDto>> GetQuizQuestionsAsync(string skillName, int count)
		{
			// Modify the prompt to request JSON output
			var prompt = $"Generate {count} multiple choice questions about {skillName}. " +
						 $"Each question should have 4 options labeled A to D. " +
						 $"Include the correct answer. " +
						 $"Format the output as a JSON array of objects, where each object has 'question', 'options' (an array of strings), and 'correctAnswer' (the text of the correct option, not just the letter).";

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

			// Gemini API endpoint for text generation
			using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}");
			// No need to add Authorization header for Gemini API key in query parameter
			request.Content = content;

			var response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				throw new Exception($"Failed to fetch quiz questions from Gemini. Status: {response.StatusCode}, Content: {errorContent}");
			}

			var json = await response.Content.ReadAsStringAsync();
			var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(json); // Changed to GeminiResponse

			// Extract the generated text from Gemini's response
			var geminiContent = geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
			return ParseQuizFromJson(geminiContent ?? string.Empty);
		}

		public async Task<QuizResultDto> SubmitQuizAsync(Guid userId, Guid skillId, List<string> answers)
		{
			var skill = await _skillRepository.GetByIdAsync(skillId);
			if (skill == null || skill.UserId != userId)
				throw new Exception("Invalid skill or user.");

			var score = GetScorefromQuestions(quizQuestionsForScoring, answers);

			string proficiencyLevel = score switch
			{
				<= 40 => "Beginner",
				<= 70 => "Intermediate",
				<= 90 => "Advanced",
				_ => "Expert"
			};

			var quizResult = new QuizResult
			{
				UserId = userId,
				SkillId = skillId,
				Score = score,
				ProficiencyLevel = proficiencyLevel,
				DateTaken = DateTime.UtcNow
			};

			skill.ProficiencyLevel = proficiencyLevel;
			skill.LastAssessed = DateTime.UtcNow;

			await _quizResultRepository.AddAsync(quizResult);
			await _skillRepository.UpdateAsync(skill);

			return new QuizResultDto
			{
				Id = quizResult.Id,
				SkillId = skillId,
				SkillName = skill.SkillName,
				Score = score,
				ProficiencyLevel = proficiencyLevel,
				DateTaken = quizResult.DateTaken
			};
		}

		private int GetScorefromQuestions(List<QuizDto> quizQuestion, List<string> answers)
		{
			int correctAnswers = quizQuestion.Zip(answers, (q, a) => q.CorrectAnswer == a ? 1 : 0).Sum();
			int score = (int)((correctAnswers / (double)quizQuestion.Count) * 100);
			return score;
		}

		private List<QuizDto> ParseQuizFromJson(string jsonText)
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
				// Fallback for strict markdown code blocks if the above fails
				cleanedJsonText = cleanedJsonText.Substring("```json".Length).Trim();
				cleanedJsonText = cleanedJsonText.Substring(0, cleanedJsonText.Length - "```".Length).Trim();
			}

			try
			{
				var quizQuestions = JsonSerializer.Deserialize<List<QuizDto>>(cleanedJsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				quizQuestionsForScoring = quizQuestions;
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
	}
}