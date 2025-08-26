namespace Skill_Matrix.Entities
{
	public class QuizBatch
	{
		public int Id { get; set; }
		public Guid SkillId { get; set; }
		public Guid UserId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public Skill Skill { get; set; }
		public User User { get; set; }

		public List<QuizQuestions> Questions { get; set; } = new();

		public List<WrongAnswers> WrongAnswers { get; set; } = new();
		public QuizResult Result { get; set; }
	}

}
