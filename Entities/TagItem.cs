using System.Text.Json.Serialization;

namespace Skill_Matrix.Entities
{
	public class TagItem
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }
	}
}
