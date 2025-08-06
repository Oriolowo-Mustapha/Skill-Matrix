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
	}
}
