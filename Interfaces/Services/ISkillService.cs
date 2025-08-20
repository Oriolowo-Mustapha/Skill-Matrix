using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Services
{
	public interface ISkillService
	{
		Task<List<SkillDto>> GetSkillsAsync(Guid userId);
		Task<SkillDto> AddSkillAsync(Guid userId, string skillName);
		Task<SkillDto> AddSkillAsync(Guid userId, string skillName, string proficiencyLevel);
		Task DeleteSkillAsync(Guid userId, Guid skillId);
		Task<(List<string> Skills, int TotalCount)> GetProgrammingSkillNamesAsync(int pageNumber, int pageSize);
		Task<PaginatedResult<string>> GetSkillsAsync(string searchTerm, int pageNumber, int pageSize = 20);
	}
}
