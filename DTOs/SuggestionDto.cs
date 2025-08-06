namespace Skill_Matrix.DTOs
{
	public class SuggestionDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid SkillId { get; set; }
		public string Suggestions { get; set; }
		public DateTime SavedAt { get; set; }
	}
}
