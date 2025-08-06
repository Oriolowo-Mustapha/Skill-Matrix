using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.Entities
{
	public class User : BaseEnitity
	{
		[Required]
		[MaxLength(50)]
		public string FirstName { get; set; }

		[Required]
		[MaxLength(50)]
		public string LastName { get; set; }

		[Required]
		[MaxLength(50)]
		public string Username { get; set; }

		[Required]
		[MaxLength(100)]
		public string Email { get; set; }

		[Required]
		public string PasswordHash { get; set; } // Hashed password for security

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public ICollection<Skill> Skills { get; set; } = new List<Skill>();
		public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
		public ICollection<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
	}
}
