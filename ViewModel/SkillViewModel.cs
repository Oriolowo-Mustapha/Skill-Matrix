using Skill_Matrix.Entities;

namespace Skill_Matrix.ViewModel
{
	public class SkillViewModel
	{
		public string Name { get; set; }
		public string ProficiencyLevel { get; set; }
		public double LatestScore { get; set; }
		public DateTime LastAssessed { get; set; }
	}

	public class SuggestionViewModel
	{
		public string SkillName { get; set; }
		public string SuggestionText { get; set; }
		public string RESourceLink { get; set; }
		public DateTime CreatedOn { get; set; }
	}

	public class AssessmentViewModel
	{
		public Guid Id { get; set; }
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public double Score { get; set; }
		public string ProficiencyLevel { get; set; }
		public DateTime TakenOn { get; set; }
		public int TotalQuestions { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int RetakeCount { get; set; }
		public List<WrongAnswers> WrongAnswers { get; set; } = new();
	}

}
