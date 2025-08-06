using Skill_Matrix.DTOs;

namespace Skill_Matrix.Interfaces.Services
{
	public interface IQuizService
	{
		Task<List<QuizDto>> GetQuizQuestionsAsync(string skillName, int count);
		Task<QuizResultDto> SubmitQuizAsync(Guid userId, Guid skillId, List<string> answers);
	}
}
