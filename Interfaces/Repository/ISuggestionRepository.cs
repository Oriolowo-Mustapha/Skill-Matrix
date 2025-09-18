using Skill_Matrix.Entities;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface ISuggestionRepository
	{
		Task AddAsync(Suggestion suggestion);
		Task<List<SuggestionVm>> SuggestionHightlights(Guid userId);
		Task<List<SuggestionViewModel>> UserSuggestions(Guid userId);
		Task<List<Suggestion>> GetSuggestionsForQuizResultAsync(Guid quizResultId);
	}
}
