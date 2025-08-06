using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skill_Matrix.Entities
{
	public class Suggestion : BaseEnitity
	{

		[ForeignKey("User")]
		public Guid UserId { get; set; }

		[ForeignKey("Skill")]
		public Guid SkillId { get; set; }

		[Required]
		[MaxLength(200)]
		public string Suggestions { get; set; }

		public DateTime SavedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public User User { get; set; }
		public Skill Skill { get; set; }
	}
}
