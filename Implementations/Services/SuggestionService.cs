using Skill_Matrix.DTOs;
using Skill_Matrix.Interfaces.Services;

namespace Skill_Matrix.Implementations.Services
{
	using Skill_Matrix.Entities;
	using Skill_Matrix.Interfaces.Repository;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Text.Json;
	using System.Threading.Tasks;

	public class SuggestionService : ISuggestionService
	{
		private readonly ISuggestionRepository _suggestionRepository;
		private readonly HttpClient _httpClient;
		private readonly string _apiKey = "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";

		public SuggestionService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<List<SuggestionDto>> GetSuggestionsAsync(string skillName, string proficiencyLevel)
		{
			string prompt = $"As a mentor, give 5 personalized suggestions to help someone at '{proficiencyLevel}' level improve their '{skillName}' skill. " +
							"Each suggestion should be concise and actionable.";

			var request = new
			{
				model = "gpt-3.5-turbo",
				messages = new[]
				{
				new { role = "user", content = prompt }
			}
			};

			var requestJson = JsonSerializer.Serialize(request);
			var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
			httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
			httpRequest.Content = content;

			var response = await _httpClient.SendAsync(httpRequest);
			var json = await response.Content.ReadAsStringAsync();

			var result = JsonSerializer.Deserialize<OpenAiResponse>(json);
			var suggestionsText = result?.choices?.FirstOrDefault()?.message?.content;

			return ParseSuggestions(suggestionsText, skillName);
		}

		public async Task SaveSuggestionAsync(SuggestionDto suggestionDto)
		{
			var suggestion = new Suggestion
			{
				UserId = suggestionDto.UserId,
				SkillId = suggestionDto.SkillId,
				Suggestions = suggestionDto.Suggestions,
				SavedAt = DateTime.UtcNow
			};

			await _suggestionRepository.AddAsync(suggestion);
		}

		private List<SuggestionDto> ParseSuggestions(string gptResponse, string skillName)
		{
			var suggestions = new List<SuggestionDto>();
			if (string.IsNullOrWhiteSpace(gptResponse)) return suggestions;

			var lines = gptResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				string cleanLine = line.Trim();

				// Remove numbering (e.g., "1.", "- ", "* ")
				if (cleanLine.Length > 2 && (char.IsDigit(cleanLine[0]) || cleanLine[0] == '-' || cleanLine[0] == '*'))
				{
					int dotIndex = cleanLine.IndexOf('.');
					if (dotIndex > 0) cleanLine = cleanLine.Substring(dotIndex + 1).Trim();
				}

				suggestions.Add(new SuggestionDto
				{
					Suggestions = cleanLine,
					SavedAt = DateTime.UtcNow
				});
			}

			return suggestions;
		}

		private class OpenAiResponse
		{
			public List<Choice> choices { get; set; }
		}

		private class Choice
		{
			public Message message { get; set; }
		}

		private class Message
		{
			public string content { get; set; }
		}
	}

}
