using Skill_Matrix.Entities;

namespace Skill_Matrix.ViewModel
{
	public class ViewResultViewModel
	{
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public DateTime DateTaken { get; set; }
		public int TotalQuestions { get; set; }
		public int Score { get; set; }
		public string ProficiencyLevel { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int RetakeCount { get; set; }
		public List<WrongAnswers> WrongAnswers { get; set; } = new();
	}
}
