using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface IQuizRepository
	{
		Task AddAsync(QuizResult quizResult);
		Task<List<QuizQuestions>> GetBySkillIdAsync(Guid userId, Guid skillId);
		Task<QuizResult> GetLatestByUserAndSkillAsync(Guid userId, Guid skillId);
		Task AddRangeAsync(List<QuizQuestions> questions);
	}
}
