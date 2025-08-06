using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.DTOs
{
	public class SkillDto
	{
		public Guid Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string SkillName { get; set; }

		public string ProficiencyLevel { get; set; }
		public DateTime? LastAssessed { get; set; }
	}
}
