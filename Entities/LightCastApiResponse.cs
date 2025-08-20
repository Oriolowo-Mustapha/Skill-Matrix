using System.Text.Json.Serialization;

namespace Skill_Matrix.Entities
{
	public class LightCastApiResponse
	{
		[JsonPropertyName("data")]
		public List<SkillName> skills { get; set; }
	}
}
