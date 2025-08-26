using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.Entities
{
	public class User : BaseEnitity
	{
		[Required, MaxLength(50)]
		public string FirstName { get; set; }

		[Required, MaxLength(50)]
		public string LastName { get; set; }

		[Required, MaxLength(50)]
		public string Username { get; set; }

		[Required, MaxLength(100)]
		public string Email { get; set; }

		[Required]
		public string PasswordHash { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation
		public List<Skill> Skills { get; set; } = new();
		public List<QuizResult> QuizResults { get; set; } = new();
		public List<Suggestion> Suggestions { get; set; } = new();
		public List<QuizBatch> QuizBatches { get; set; } = new();
	}
}
