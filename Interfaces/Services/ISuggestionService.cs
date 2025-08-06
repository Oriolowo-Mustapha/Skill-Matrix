using Skill_Matrix.DTOs;

namespace Skill_Matrix.Interfaces.Services
{
	public interface ISuggestionService
	{
		Task<List<SuggestionDto>> GetSuggestionsAsync(string skillName, string proficiencyLevel);
		Task SaveSuggestionAsync(SuggestionDto suggestionDto);
	}
}
