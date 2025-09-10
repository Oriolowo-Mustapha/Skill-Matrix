using Skill_Matrix.Entities;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface IQuizRepository
	{
		Task AddAsync(QuizResult quizResult);
		Task<List<QuizQuestions>> GetBySkillIdAsync(Guid userId, Guid skillId);
		Task<bool> GetResultBySkillId(Guid userId, Guid skillId);
		Task<QuizResult> GetLatestByUserAndSkillAsync(Guid userId, Guid skillId);
		Task AddQuestionAsync(QuizBatch questions);
		Task<List<QuizResult?>> GetResultByUserId(Guid userId);
		Task<double> GetAverageByUserId(Guid userId);
		Task<QuizResult?> GetQuizResultById(Guid QuizResultId);
		Task<(string BestSkill, string WeakestSkill)> GetBestAndWeakestSkillsAsync(Guid userId);
		Task<QuizResult> GetWeakestQuizByUserId(Guid userId);
		Task<List<QuizQuestions>> GetByBatchId(int BatchId);
		Task<List<AssessmentSummaryVm>> RecentAssesment(Guid UserId);
		Task<List<AssessmentTrendVm>> AssessmentTrends(Guid UserId);
		Task<List<AssessmentViewModel>> UserAssessments(Guid userId);
	}
}
