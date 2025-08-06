namespace Skill_Matrix.Entities
{
	public class Skill : BaseEnitity
	{
		public string SkillName { get; set; } // e.g., "C#", "JavaScript"
		public string ProficiencyLevel { get; set; } // e.g., "Beginner", "Intermediate"
		public DateTime? LastAssessed { get; set; } // Date of last quiz
		public Guid UserId { get; set; } // Foreign key to User

		// Navigation properties
		public User User { get; set; }
		public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
		public ICollection<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
	}
}
