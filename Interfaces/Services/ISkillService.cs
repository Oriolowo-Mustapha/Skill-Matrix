using Skill_Matrix.DTOs;

namespace Skill_Matrix.Interfaces.Services
{
	public interface ISkillService
	{
		Task<List<SkillDto>> GetSkillsAsync(Guid userId);
		Task<SkillDto> AddSkillAsync(Guid userId, string skillName);
		Task<List<string>> GetSkillNamesOnlyAsync();
		Task DeleteSkillAsync(Guid userId, Guid skillId);
	}
}
