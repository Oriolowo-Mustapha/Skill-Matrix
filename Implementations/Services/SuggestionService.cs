using Microsoft.Extensions.Caching.Memory;
using Skill_Matrix.Data;
using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using System.Text;
using System.Text.Json;
namespace Skill_Matrix.Implementations.Services;

public class SuggestionService : ISuggestionService
{
	private readonly IQuizRepository _quizRepository;
	private readonly ISuggestionRepository _suggestionRepository;
	private readonly ISkillRepository _skillRepository;
	private readonly HttpClient _httpClient;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly string _apiKey;
	private readonly IMemoryCache _memoryCache;
	private readonly SkillMatrixDbContext _dbContext;

	public SuggestionService(
		IQuizRepository quizRepository,
		ISuggestionRepository suggestionRepository,
		ISkillRepository skillRepository,
		IConfiguration configuration,
		HttpClient httpClient,
		IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, SkillMatrixDbContext dbContext)
	{
		_quizRepository = quizRepository;
		_suggestionRepository = suggestionRepository;
		_skillRepository = skillRepository;
		_httpClient = httpClient;
		_httpContextAccessor = httpContextAccessor;
		_memoryCache = memoryCache;
		_dbContext = dbContext;

		// ✅ get API key from appsettings.json or Render environment variables
		_apiKey = configuration["Gemini:ApiKey"];
	}
	public async Task<List<SuggestionDto>> GetSuggestionsAsync(Guid QuizResultId)
	{

		var Result = await _quizRepository.GetQuizResultById(QuizResultId);
		if (Result == null)
		{
			return new List<SuggestionDto>
		{
			new SuggestionDto { Suggestions = "Quiz result not found.", ResourseLienk = "" }
		};
		}

		var Skill = await _skillRepository.GetByIdAsync(Result.SkillId);
		var skillName = Skill.SkillName;
		var ProficiencyLevel = Result.ProficiencyLevel;

		var Question = await _quizRepository.GetByBatchId(Result.QuizBatchId);
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
			$"Based on the following criteria, generate ** highly relevant learning suggestions**. " +
			$"Each suggestion must directly address the user's weaknesses and proficiency level for user skill development include a link to a high-quality, free, and accessible online resource (e.g., a video, tutorial, or documentation)." +
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
			$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}"
		);
		request.Content = content;

		var response = await _httpClient.SendAsync(request);

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
				UserId = GetCurrentUserId(),
				SkillId = Skill.Id,
				QuizResultId = QuizResultId,
				Suggestions = item.Suggestions,
				ResourceLink = item.ResourseLienk,
				SavedAt = DateTime.UtcNow

			};
			await _suggestionRepository.AddAsync(suggest);
		}
		_memoryCache.Set($"Suggestions_{QuizResultId}", suggestions, TimeSpan.FromHours(1));
		await SaveSuggestionAsync(QuizResultId, suggestions);
		return suggestions;
	}

	public async Task SaveSuggestionAsync(Guid quizResultId, List<SuggestionDto> suggestionDto)
	{
		var UserId = GetCurrentUserId();
		var quizResult = await _quizRepository.GetQuizResultById(quizResultId);

		foreach (var dto in suggestionDto)
		{
			var suggestion = new Suggestion
			{
				Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
				UserId = UserId,
				SkillId = quizResult.SkillId,
				QuizResultId = quizResultId,
				ResourceLink = dto.ResourseLienk,
				Suggestions = dto.Suggestions,
				SavedAt = DateTime.UtcNow
			};

			await _suggestionRepository.AddAsync(suggestion);
		}
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
	public List<SuggestionDto> GetSuggestionsFromCache(Guid QuizResultId)
	{
		var cacheKey = $"Suggestions_{QuizResultId}";
		if (!_memoryCache.TryGetValue(cacheKey, out List<Suggestion> suggestions))
		{
			var suggestion = _dbContext.Suggestions
				.Where(s => s.QuizResultId == QuizResultId)
				.ToList();

			suggestions = suggestion ?? new List<Suggestion>();

			// Save into cache

			_memoryCache.Set(cacheKey, suggestions, TimeSpan.FromHours(1));
		}
		var SuggestDto = suggestions.Select(s => new SuggestionDto
		{
			Id = s.Id,
			ResourseLienk = s.ResourceLink,
			Suggestions = s.Suggestions,
			SavedAt = s.SavedAt
		}).ToList();

		return SuggestDto;
	}


	private Guid GetCurrentUserId()
	{
		var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
		if (string.IsNullOrEmpty(userIdString))
			throw new UnauthorizedAccessException("User not logged in.");

		return Guid.Parse(userIdString);
	}
}


