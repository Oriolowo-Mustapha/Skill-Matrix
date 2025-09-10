using Skill_Matrix.Entities;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface ISkillRepository
	{
		Task<List<Skill>> GetByUserIdAsync(Guid userId);
		Task<Skill> GetByUserIdandSkillNameAsync(Guid userId, string skillName);
		Task<List<SkillPerformanceVm>> SkillPerformace(Guid userId);
		Task<List<SkillViewModel>> UserSkills(Guid userId);
		Task<Skill> GetByIdAsync(Guid id);
		Task AddAsync(Skill skill);
		Task DeleteAsync(Skill skill);
		Task UpdateAsync(Skill skill);
	}
}
