using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skill_Matrix.Entities
{
	public class QuizResult : BaseEnitity
	{
		[ForeignKey("User")]
		public Guid UserId { get; set; }

		[ForeignKey("Skill")]
		public Guid SkillId { get; set; }

		public int Score { get; set; } // Score as percentage (0-100)

		[MaxLength(50)]
		public string ProficiencyLevel { get; set; } // e.g., "Beginner", "Intermediate"

		public DateTime DateTaken { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public User User { get; set; }
		public Skill Skill { get; set; }
	}
}
