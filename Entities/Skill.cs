namespace Skill_Matrix.Entities
{
	public class Skill : BaseEnitity
	{
		public string SkillName { get; set; }
		public string ProficiencyLevel { get; set; }
		public DateTime? LastAssessed { get; set; }

		public Guid UserId { get; set; }
		public User User { get; set; }

		public List<QuizResult> QuizResults { get; set; } = new();
		public List<Suggestion> Suggestions { get; set; } = new();
		public List<QuizQuestions> QuizQuestions { get; set; } = new();
	}
}
