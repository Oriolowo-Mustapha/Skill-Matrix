using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.ViewModel;

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

		public async Task<List<SuggestionVm>> SuggestionHightlights(Guid userId)
		{
			var topSuggestions = await _context.Suggestions
			.Include(s => s.Skill)
			.Where(s => s.UserId == userId)
			.OrderByDescending(s => s.SavedAt) // assuming BaseEntity has CreatedAt
			.Take(4)
			.Select(s => new SuggestionVm
			{
				SkillName = s.Skill.SkillName,
				ImprovementTip = s.Suggestions // assuming you store suggestion text in Description
			})
			.ToListAsync();

			return topSuggestions;
		}

		public async Task<List<SuggestionViewModel>> UserSuggestions(Guid userId)
		{
			var suggestions = await _context.Suggestions
				.Include(s => s.Skill)
				.Where(s => s.UserId == userId)
				.OrderByDescending(s => s.SavedAt)
				.Select(s => new SuggestionViewModel
				{
					SkillName = s.Skill.SkillName,
					SuggestionText = s.Suggestions,
					RESourceLink = s.ResourceLink,
					CreatedOn = s.SavedAt
				})
				.ToListAsync();

			return suggestions;
		}

	}
}
