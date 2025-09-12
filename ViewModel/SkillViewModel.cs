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
		public string SkillName { get; set; }
		public double Score { get; set; }
		public string ProficiencyLevel { get; set; }
		public DateTime TakenOn { get; set; }
	}

}
