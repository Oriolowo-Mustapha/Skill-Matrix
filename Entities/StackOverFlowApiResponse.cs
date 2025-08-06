using System.Text.Json.Serialization;

namespace Skill_Matrix.Entities
{
	public class StackOverFlowApiResponse
	{
		[JsonPropertyName("items")]
		public List<TagItem> Items { get; set; }
	}
}
