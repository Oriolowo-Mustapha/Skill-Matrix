namespace Skill_Matrix.Entities
{
	public class PaginatedResult<T>
	{
		public List<T> Items { get; set; } = new();
		public int TotalPages { get; set; }
		public int TotalCount { get; set; }
	}
}
