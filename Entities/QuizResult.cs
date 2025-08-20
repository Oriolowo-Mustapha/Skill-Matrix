using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.Entities
{
	public class QuizResult : BaseEnitity
	{
		public Guid UserId { get; set; }
		public User User { get; set; }

		public Guid SkillId { get; set; }
		public Skill Skill { get; set; }

		public int Score { get; set; }
		[MaxLength(50)]
		public string ProficiencyLevel { get; set; }
		public int RetakeCount { get; set; } = 0;
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }

		public DateTime DateTaken { get; set; } = DateTime.UtcNow;

		public List<QuizQuestions> QuizQuestions { get; set; } = new();
		public List<Suggestion> Suggestions { get; set; } = new();
	}
}
