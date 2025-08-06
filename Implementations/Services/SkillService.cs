using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using System.Text.Json;

namespace Skill_Matrix.Implementations.Services
{
	public class SkillService : ISkillService
	{
		private readonly ISkillRepository _skillRepository;
		private readonly HttpClient _httpClient;

		public SkillService(ISkillRepository skillRepository, HttpClient httpClient)
		{
			_skillRepository = skillRepository;
			_httpClient = httpClient;
		}

		public async Task<SkillDto> AddSkillAsync(Guid userId, string skillName)
		{
			if (string.IsNullOrWhiteSpace(skillName))
				throw new ArgumentException("Skill name cannot be empty.");


			// 3. Create and Save Skill
			var skill = new Skill
			{
				UserId = userId,
				SkillName = skillName.Trim(), // Trim whitespace
				ProficiencyLevel = "Null",
			};

			await _skillRepository.AddAsync(skill);

			// 4. Return DTO
			return new SkillDto
			{
				Id = skill.Id,
				SkillName = skill.SkillName,
				ProficiencyLevel = skill.ProficiencyLevel,
				LastAssessed = skill.LastAssessed
			};
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

		public async Task<List<string>> GetSkillNamesOnlyAsync()
		{
			try
			{
				var url = $"https://api.stackexchange.com/2.3/tags?pagesize=50&order=desc&sort=popular&site=stackoverflow";

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Add("User-Agent", "SkillMatrixApp/1.0");

				var response = await _httpClient.SendAsync(request);

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception("Error Fetching Skills From API.");
				}

				var json = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<StackOverFlowApiResponse>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return result?.Items?.Select(tag => tag.Name).ToList() ?? new List<string>();
			}
			catch (Exception ex)
			{
				throw new Exception("Error Getting Response. " + ex.Message);
			}
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
