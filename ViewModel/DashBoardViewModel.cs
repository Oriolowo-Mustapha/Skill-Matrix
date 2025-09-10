namespace Skill_Matrix.ViewModel
{
	public class DashBoardViewModel
	{
		// User Greeting
		public string Username { get; set; }
		public DateTime MemberSince { get; set; }

		// Key Stats
		public int TotalSkills { get; set; }
		public double AverageScore { get; set; }
		public string BestSkill { get; set; }
		public string WeakestSkill { get; set; }
		public int TotalAssessments { get; set; }

		// Recent Activity
		public List<AssessmentSummaryVm> RecentAssessments { get; set; } = new();

		// Skill Progress Overview
		public List<SkillPerformanceVm> SkillPerformances { get; set; } = new();

		// Suggestions Highlights
		public List<SuggestionVm> TopSuggestions { get; set; } = new();

		// Trend Data
		public List<AssessmentTrendVm> AssessmentTrends { get; set; } = new();

	}

	public class AssessmentSummaryVm
	{
		public Guid AssessmentId { get; set; }
		public string SkillName { get; set; }
		public double Score { get; set; }
		public string Level { get; set; }
		public DateTime TakenOn { get; set; }
	}

	public class SkillPerformanceVm
	{
		public string SkillName { get; set; }
		public double AverageScore { get; set; }
		public string CurrentLevel { get; set; }
	}

	public class SuggestionVm
	{
		public string SkillName { get; set; }
		public string ImprovementTip { get; set; }
	}

	public class AssessmentTrendVm
	{
		public DateTime Date { get; set; }
		public double Score { get; set; }
	}
}
