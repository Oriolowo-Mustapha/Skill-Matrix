using Skill_Matrix.DTOs;

namespace Skill_Matrix.Interfaces.Services
{
	public interface ISuggestionService
	{
		Task<List<SuggestionDto>> GetSuggestionsAsync(Guid QuizResultId);
		Task SaveSuggestionAsync(Guid quizResultId, List<SuggestionDto> suggestionDto);
		public List<SuggestionDto> GetSuggestionsFromCache(Guid QuizResultId);
	}
}
