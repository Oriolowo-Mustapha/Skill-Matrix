using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface ISkillRepository
	{
		Task<List<Skill>> GetByUserIdAsync(Guid userId);
		Task<Skill> GetByUserIdandSkillNameAsync(Guid userId, string skillName);
		Task<Skill> GetByIdAsync(Guid id);
		Task AddAsync(Skill skill);
		Task DeleteAsync(Skill skill);
		Task UpdateAsync(Skill skill);
	}
}
