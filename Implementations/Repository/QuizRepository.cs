using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;

namespace Skill_Matrix.Implementations.Repository
{
	public class QuizRepository : IQuizRepository
	{
		private readonly SkillMatrixDbContext _context;

		public QuizRepository(SkillMatrixDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(QuizResult quizResult)
		{
			await _context.QuizResults.AddAsync(quizResult);
			await _context.SaveChangesAsync();
		}

		public async Task<List<QuizQuestions>> GetBySkillIdAsync(Guid userId, Guid skillId)
		{
			return await _context.QuizQuestions
				.Where(q => q.SkillId == skillId && q.UserId == userId)
				.OrderByDescending(q => q.CreatedAt)
				.Include(q => q.Options)
				.ToListAsync();
		}
		public async Task<QuizResult> GetLatestByUserAndSkillAsync(Guid userId, Guid skillId)
		{
			return await _context.QuizResults
				.Where(r => r.UserId == userId && r.SkillId == skillId)
				.OrderByDescending(r => r.DateTaken) // latest attempt
				.Include(r => r.QuizQuestions)
					.ThenInclude(q => q.WrongAnswers)
				.FirstOrDefaultAsync();
		}

		public async Task AddRangeAsync(List<QuizQuestions> questions)
		{
			await _context.QuizQuestions.AddRangeAsync(questions);
			await _context.SaveChangesAsync();
		}
	}
}
