namespace Skill_Matrix.Entities
{
	public class QuizQuestions
	{
		public int Id { get; set; }
		public string Question { get; set; }
		public string CorrectAnswer { get; set; }

		public Guid SkillId { get; set; }
		public Skill Skill { get; set; }

		public Guid UserId { get; set; }
		public User User { get; set; }

		public List<Options> Options { get; set; } = new();
		public List<WrongAnswers> WrongAnswers { get; set; } = new();
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
