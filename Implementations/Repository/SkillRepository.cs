using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.ViewModel;

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

		public async Task<Skill> GetByUserIdandSkillNameAsync(Guid userId, string skillName)
		{
			return await _context.Skills.
				Where(s => s.UserId == userId && s.SkillName == skillName).FirstOrDefaultAsync();
		}

		public async Task<List<SkillPerformanceVm>> SkillPerformace(Guid userId)
		{
			var skillPerformances = await _context.QuizResults
			.Include(q => q.Skill)
			.Where(q => q.UserId == userId)
			.OrderByDescending(q => q.Score)
			.GroupBy(q => new { q.SkillId, q.Skill.SkillName, q.Skill.ProficiencyLevel })
			.Take(5)
			.Select(g => new SkillPerformanceVm
			{
				SkillName = g.Key.SkillName,
				AverageScore = g.Average(r => r.Score),
				CurrentLevel = g.Key.ProficiencyLevel
			})
			.ToListAsync();
			return skillPerformances;
		}

		public async Task<List<SkillViewModel>> UserSkills(Guid userId)
		{
			var skills = await _context.Skills
				.Include(s => s.QuizResults)
				.Where(s => s.UserId == userId)
				.Select(s => new SkillViewModel
				{
					Name = s.SkillName,
					ProficiencyLevel = s.ProficiencyLevel,
					LatestScore = s.QuizResults
									.OrderByDescending(q => q.DateTaken)
									.Select(q => (double)q.Score)
									.FirstOrDefault(),
					LastAssessed = s.QuizResults
									.OrderByDescending(q => q.DateTaken)
									.Select(q => q.DateTaken)
									.FirstOrDefault()
				})
				.ToListAsync();

			return skills;
		}

	}
}
