using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;

namespace Skill_Matrix.Implementations.Repository
{
	public class WrongAnswerRepository(SkillMatrixDbContext context) : IWrongAnswersRepository
	{
		public async Task<List<WrongAnswers>> GetByBatchId(int BatchId)
		{
			return await context.WrongAnswers
				.Where(w => w.QuizBatchId == BatchId)
				.ToListAsync();
		}
	}
}