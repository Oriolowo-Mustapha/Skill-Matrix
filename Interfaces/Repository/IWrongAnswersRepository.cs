using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface IWrongAnswersRepository
	{
		Task<List<WrongAnswers>> GetByBatchId(int BatchId);
	}
}
