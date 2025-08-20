namespace Skill_Matrix.ViewModel
{
	public class PagedSkillsViewModel
	{
		public List<string> Skills { get; set; }
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
		public bool HasPreviousPage { get; set; }
		public bool HasNextPage { get; set; }
	}
}
