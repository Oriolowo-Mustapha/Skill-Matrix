using System.Text.Json.Serialization;

namespace Skill_Matrix.Entities
{
	public class GeminiResponse
	{
		[JsonPropertyName("candidates")]
		public List<Candidate> candidates { get; set; }
	}
	public class Candidate
	{
		[JsonPropertyName("content")]
		public Content content { get; set; }
	}

	public class Content
	{
		[JsonPropertyName("parts")]
		public List<Part> parts { get; set; }
	}

	public class Part
	{
		[JsonPropertyName("text")]
		public string text { get; set; }
	}
}
