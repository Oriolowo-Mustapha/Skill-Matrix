using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;

namespace Skill_Matrix.Implementations.Repository
{
	public class SkillRepository : ISkillRepository
	{
		private readonly SkillMatrixDbContext _context;

		public SkillRepository(SkillMatrixDbContext context)
		{
			_context = context;
		}

		public async Task<List<Skill>> GetByUserIdAsync(Guid userId)
		{
			return await _context.Skills
				.Where(s => s.UserId == userId)
				.ToListAsync();
		}

		public async Task<Skill> GetByIdAsync(Guid id)
		{
			return await _context.Skills.FindAsync(id);
		}

		public async Task AddAsync(Skill skill)
		{
			await _context.Skills.AddAsync(skill);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Skill skill)
		{
			_context.Skills.Remove(skill);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Skill skill)
		{
			_context.Skills.Update(skill);
			await _context.SaveChangesAsync();
		}
	}
}
