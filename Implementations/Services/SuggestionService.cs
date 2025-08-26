using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using System.Text;
using System.Text.Json;
namespace Skill_Matrix.Implementations.Services;

public class SuggestionService(IQuizRepository quizRepository, ISuggestionRepository suggestionRepository, ISkillRepository skillRepository, IConfiguration configuration, string geminiApiKey, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : ISuggestionService
{

	public async Task<List<SuggestionDto>> GetSuggestionsAsync(Guid QuizResultId)
	{
		geminiApiKey = configuration["Gemini:ApiKey"];

		var Result = await quizRepository.GetQuizResultById(QuizResultId);
		if (Result == null)
		{
			return new List<SuggestionDto>
		{
			new SuggestionDto { Suggestions = "Quiz result not found.", ResourseLienk = "" }
		};
		}

		var Skill = await skillRepository.GetByIdAsync(Result.SkillId);
		var skillName = Skill.SkillName;
		var ProficiencyLevel = Result.ProficiencyLevel;

		var Question = await quizRepository.GetByBatchId(Result.QuizBatchId);
		var WrongAnswer = new List<string>();
		foreach (var item in Question)
		{
			if (item.WrongAnswers == null || !item.WrongAnswers.Any())
			{
				WrongAnswer.Add(item.Question);
			}
		}
		var WrongAnswersText = string.Join(" ", WrongAnswer);

		var prompt =
			$"You are an expert, supportive, and motivating learning guide. " +
			$"Your purpose is to help a user learn a new skill and address their specific weaknesses. " +
			$"Based on the following criteria, generate **exactly 3 concise, highly relevant learning suggestions**. " +
			$"Each suggestion must directly address the user's weaknesses and include a link to a high-quality, free, and accessible online resource (e.g., a video, tutorial, or documentation)." +
			$"\n\n**Criteria:**" +
			$"\n1.  **Skill:** {skillName}" +
			$"\n2.  **User Proficiency Level:** {ProficiencyLevel}" +
			$"\n3.  **Core Weaknesses:** Based on these incorrect quiz questions: {WrongAnswersText}" +
			$"\n\n**Response Format:**" +
			$"\nReturn only valid JSON with this structure: " +
			$"[\n" +
			$"  {{ \"Suggestions\": \"string\", \"ResourseLienk\": \"string\" }}," +
			$"  {{ \"Suggestions\": \"string\", \"ResourseLienk\": \"string\" }}," +
			$"  {{ \"Suggestions\": \"string\", \"ResourseLienk\": \"string\" }}\n" +
			$"]" +
			$"\nDo not include any extra text, explanations, or formatting outside of the JSON array.";

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

		using var request = new HttpRequestMessage(
			HttpMethod.Post,
			$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={geminiApiKey}"
		);
		request.Content = content;

		var response = await httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync();
			throw new Exception($"Failed to fetch suggestion from Gemini. Status: {response.StatusCode}, Content: {errorContent}");
		}

		var json = await response.Content.ReadAsStringAsync();
		var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(json);

		var geminiContent = geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;

		if (string.IsNullOrWhiteSpace(geminiContent))
		{
			return new List<SuggestionDto>
			{
				new SuggestionDto { Suggestions = "Gemini returned no content.", ResourseLienk = "" }
			};
		}

		// Parse into DTO list
		var suggestions = ParseSuggestionsJson(geminiContent);
		foreach (var item in suggestions)
		{
			var suggest = new Suggestion()
			{
				Id = item.Id,
			};
			await suggestionRepository.AddAsync(item);
		}

		return suggestions;
	}

	public async Task SaveSuggestionAsync(Guid quizResultId, SuggestionDto suggestionDto)
	{
		var UserId = GetCurrentUserId();
		var quizResult = await quizRepository.GetQuizResultById(quizResultId);
		var suggestion = new Suggestion
		{
			Id = suggestionDto.Id != Guid.Empty ? suggestionDto.Id : Guid.NewGuid(),
			UserId = UserId,
			SkillId = quizResult.SkillId,
			QuizResultId = quizResultId,
			ResourceLink = suggestionDto.ResourseLienk,
			Suggestions = suggestionDto.Suggestions,
			SavedAt = DateTime.UtcNow
		};

		await suggestionRepository.AddAsync(suggestion);
	}


	private List<SuggestionDto> ParseSuggestionsJson(string geminiContent)
	{
		try
		{
			geminiContent = geminiContent.Trim();

			// Strip code fences if Gemini adds them
			if (geminiContent.StartsWith("```"))
			{
				int firstBrace = geminiContent.IndexOf('[');
				int lastBrace = geminiContent.LastIndexOf(']');
				if (firstBrace >= 0 && lastBrace > firstBrace)
				{
					geminiContent = geminiContent.Substring(firstBrace, lastBrace - firstBrace + 1);
				}
			}

			var dtos = JsonSerializer.Deserialize<List<SuggestionDto>>(geminiContent, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			return dtos ?? new List<SuggestionDto>
			{
				new SuggestionDto { Suggestions = "No suggestions generated.", ResourseLienk = "" }
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[ParseSuggestionsJson] Failed: {ex.Message}\nContent: {geminiContent}");

			return new List<SuggestionDto>
			{
				new SuggestionDto { Suggestions = "Could not parse suggestions.", ResourseLienk = "" }
			};
		}
	}

	private Guid GetCurrentUserId()
	{
		var userIdString = httpContextAccessor.HttpContext?.Session.GetString("UserId");
		if (string.IsNullOrEmpty(userIdString))
			throw new UnauthorizedAccessException("User not logged in.");

		return Guid.Parse(userIdString);
	}
}


