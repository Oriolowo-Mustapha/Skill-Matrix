using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.Entities
{
	public class Suggestion : BaseEnitity
	{
		public Guid UserId { get; set; }
		public User User { get; set; }

		public Guid SkillId { get; set; }
		public Skill Skill { get; set; }

		public Guid QuizResultId { get; set; }
		public QuizResult QuizResult { get; set; }

		[Required, MaxLength(200)]
		public string Suggestions { get; set; }

		[Required, MaxLength(200)]
		public string ResourceLink { get; set; }

		public DateTime SavedAt { get; set; } = DateTime.UtcNow;
	}
}

