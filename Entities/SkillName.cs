using System.Text.Json.Serialization;

namespace Skill_Matrix.Entities
{
	public class SkillName
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }
	}
}
