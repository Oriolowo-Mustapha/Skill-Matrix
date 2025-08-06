using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface IQuizRepository
	{
		Task AddAsync(QuizResult quizResult);
	}
}
