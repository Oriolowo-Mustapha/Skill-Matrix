namespace Skill_Matrix.DTOs
{
	public class QuizDto
	{
		public string Question { get; set; }
		public List<string> Options { get; set; }
		public string CorrectAnswer { get; set; } // For validation (not shown to user)
	}

	public class QuizResultDto
	{
		public Guid Id { get; set; }
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public int Score { get; set; }
		public string ProficiencyLevel { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int RetakeCount { get; set; }
		public DateTime DateTaken { get; set; }
	}
}
