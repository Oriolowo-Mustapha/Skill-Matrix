using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;

namespace Skill_Matrix.Implementations.Repository
{
	public class SuggestionRepository : ISuggestionRepository
	{
		private readonly SkillMatrixDbContext _context;

		public SuggestionRepository(SkillMatrixDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Suggestion suggestion)
		{
			await _context.Suggestions.AddAsync(suggestion);
			await _context.SaveChangesAsync();
		}
	}
}
