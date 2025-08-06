using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface ISuggestionRepository
	{
		Task AddAsync(Suggestion suggestion);
	}
}
