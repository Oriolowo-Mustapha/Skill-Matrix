using Skill_Matrix.DTOs;

namespace Skill_Matrix.Interfaces.Services
{
	public interface ISuggestionService
	{
		Task<List<SuggestionDto>> GetSuggestionsAsync(Guid QuizResultId);
		Task SaveSuggestionAsync(Guid quizResultId, SuggestionDto suggestionDto);
	}
}
