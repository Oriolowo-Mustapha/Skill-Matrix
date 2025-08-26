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
				.Where(q => q.QuizBatch.SkillId == skillId && q.QuizBatch.UserId == userId)
				.OrderByDescending(q => q.QuizBatch.CreatedAt)
				.Include(q => q.Options)
				.Include(q => q.WrongAnswers)
				.ToListAsync();
		}

		public async Task<QuizResult> GetQuizResultById(Guid QuizResultId)
		{
			return await _context.QuizResults
				.Where(q => q.Id == QuizResultId)
				.Include(q => q.Skill)
				.FirstOrDefaultAsync();
		}

		public async Task<QuizResult> GetLatestByUserAndSkillAsync(Guid userId, Guid skillId)
		{
			return await _context.QuizResults
				.Where(r => r.QuizBatch.UserId == userId && r.QuizBatch.SkillId == skillId)
				.OrderByDescending(r => r.DateTaken)
				.Include(r => r.QuizBatch)
					.ThenInclude(b => b.Questions)
						.ThenInclude(q => q.Options)
				.Include(r => r.QuizBatch)
					.ThenInclude(b => b.Questions)
						.ThenInclude(q => q.WrongAnswers)
				.FirstOrDefaultAsync();
		}


		public async Task AddQuestionAsync(QuizBatch questions)
		{
			await _context.QuizBatches.AddRangeAsync(questions);
			await _context.SaveChangesAsync();
		}

		public async Task<List<QuizQuestions>> GetByBatchId(int BatchId)
		{
			return await _context.QuizQuestions
				.Where(q => q.QuizBatchId == BatchId)
				.Include(q => q.WrongAnswers)
				.ToListAsync();
		}
	}
}
