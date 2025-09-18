using Skill_Matrix.DTOs;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Interfaces.Services
{
	public interface IQuizService
	{
		Task<QuizBatchDto> GetQuizQuestionsAsync(string skillName, int count, string ProficiencyLevel);
		Task<QuizResultDto> SubmitQuizAsync(Guid userId, Guid skillId, int quizBatchId, List<string> answers);
		List<QuizDto> GetQuizQuestionsFromCache(Guid skillId);
		Task<List<AssessmentViewModel>> UsersAssesment(Guid userId);
		Task<QuizBatchDto> CreateRetakeQuizAsync(Guid userId, Guid skillId);
	}
}
