using Microsoft.Extensions.Caching.Memory;
using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Skill_Matrix.Implementations.Services
{
	public class SkillService : ISkillService
	{
		private readonly string _clientId;
		private readonly string _clientSecret;
		private readonly ISkillRepository _skillRepository;
		private readonly IQuizRepository _quizRepository;
		private readonly HttpClient _httpClient;
		private readonly IMemoryCache _cache;

		public SkillService(
			ISkillRepository skillRepository,
			HttpClient httpClient,
			IConfiguration configuration,
			IMemoryCache cache, IQuizRepository quizRepository)
		{
			_clientId = configuration["Lightcast:clientId"];
			_clientSecret = configuration["Lightcast:clientSecret"];
			_skillRepository = skillRepository;
			_httpClient = httpClient;
			_quizRepository = quizRepository;
			_cache = cache;
		}
		public async Task<SkillDto> AddSkillAsync(Guid userId, string skillName, string proficiencyLevel)
		{
			// Validate input parameters
			if (string.IsNullOrWhiteSpace(skillName))
				throw new ArgumentException("Skill name cannot be empty.", nameof(skillName));

			if (string.IsNullOrWhiteSpace(proficiencyLevel))
				throw new ArgumentException("Proficiency level cannot be empty.", nameof(proficiencyLevel));

			// Validate proficiency level against allowed values
			var validProficiencyLevels = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
			if (!validProficiencyLevels.Contains(proficiencyLevel))
				throw new ArgumentException($"Invalid proficiency level. Must be one of: {string.Join(", ", validProficiencyLevels)}", nameof(proficiencyLevel));

			// Check if user already has this skill (optional - depends on your business rules)
			var existingSkill = await _skillRepository.GetByUserIdandSkillNameAsync(userId, skillName.Trim());
			if (existingSkill != null)
			{
				var existResult = await _quizRepository.GetResultBySkillId(userId, existingSkill.Id);
				if (existResult)
				{
					throw new Exception("Skill Already Added By User Pls Choose Another.");
				}
			}


			Skill skill;
			if (existingSkill != null)
			{
				// If the skill already exists, update it
				existingSkill.ProficiencyLevel = proficiencyLevel;
				existingSkill.LastAssessed = DateTime.UtcNow;
				await _skillRepository.UpdateAsync(existingSkill);
				skill = existingSkill;
			}
			else
			{
				// If the skill doesn't exist, create a new one
				skill = new Skill
				{
					UserId = userId,
					SkillName = skillName.Trim(),
					ProficiencyLevel = proficiencyLevel,
					LastAssessed = DateTime.UtcNow
				};
				await _skillRepository.AddAsync(skill);
			}

			// Return DTO
			return new SkillDto
			{
				Id = skill.Id,
				SkillName = skill.SkillName,
				ProficiencyLevel = skill.ProficiencyLevel,
				LastAssessed = skill.LastAssessed
			};
		}

		// Keep the original method for backward compatibility if needed elsewhere
		public async Task<SkillDto> AddSkillAsync(Guid userId, string skillName)
		{
			return await AddSkillAsync(userId, skillName, "Beginner");
		}

		public async Task DeleteSkillAsync(Guid userId, Guid skillId)
		{
			var skill = await _skillRepository.GetByIdAsync(skillId);
			if (skill == null || skill.UserId != userId)
			{
				throw new Exception("Skill not found or unauthorized.");
			}

			await _skillRepository.DeleteAsync(skill);
		}


		private async Task<string> GetAccessTokenAsync()
		{
			if (_cache.TryGetValue("Lightcast_AccessToken", out string cachedToken))
			{
				return cachedToken;
			}

			var tokenUrl = "https://auth.emsicloud.com/connect/token";
			var body = $"client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials&scope=emsi_open";

			var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
			{
				Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
			};

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var tokenData = JsonSerializer.Deserialize<TokenResponse>(json);

			// Cache token for its lifetime minus 60 seconds
			_cache.Set("Lightcast_AccessToken", tokenData.access_token, TimeSpan.FromSeconds(tokenData.expires_in - 60));

			return tokenData.access_token;
		}

		public async Task<(List<string> Skills, int TotalCount)> GetProgrammingSkillNamesAsync(int pageNumber, int pageSize)
		{
			// Load from cache if possible
			if (!_cache.TryGetValue("AllProgrammingSkills", out List<string> allSkills))
			{
				allSkills = await FetchAllSkillsFromApi();
				_cache.Set("AllProgrammingSkills", allSkills, TimeSpan.FromMinutes(30));
			}

			var totalCount = allSkills.Count;
			var skillsOnPage = allSkills
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return (skillsOnPage, totalCount);
		}

		private async Task<List<string>> FetchAllSkillsFromApi()
		{
			var token = await GetAccessTokenAsync();
			var allSkills = new List<string>();

			// Fetch max allowed at once (API does not support offset)
			var url = "https://emsiservices.com/skills/versions/latest/skills?limit=1000&q=programming";
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			using var jsonStream = await response.Content.ReadAsStreamAsync();
			var jsonDoc = await JsonSerializer.DeserializeAsync<JsonElement>(jsonStream);

			if (jsonDoc.TryGetProperty("data", out JsonElement dataElement))
			{
				foreach (var skill in dataElement.EnumerateArray())
				{
					if (skill.TryGetProperty("name", out JsonElement nameElement))
					{
						allSkills.Add(nameElement.GetString());
					}
				}
			}

			return allSkills;
		}

		private class TokenResponse
		{
			public string access_token { get; set; }
			public int expires_in { get; set; }
		}

		public async Task<PaginatedResult<string>> GetSkillsAsync(string searchTerm, int pageNumber, int pageSize = 20)
		{
			var allSkills = await FetchAllSkillsFromApi();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				allSkills = allSkills
					.Where(s => s.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			var totalCount = allSkills.Count;
			var skillsOnPage = allSkills
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			return new PaginatedResult<string>
			{
				Items = skillsOnPage,
				TotalPages = totalPages,
				TotalCount = totalCount
			};
		}

		public async Task<List<SkillDto>> GetSkillsAsync(Guid userId)
		{
			var skills = await _skillRepository.GetByUserIdAsync(userId);
			return skills.Select(s => new SkillDto
			{
				Id = s.Id,
				SkillName = s.SkillName,
				ProficiencyLevel = s.ProficiencyLevel,
				LastAssessed = s.LastAssessed
			}).ToList();
		}
	}
}
