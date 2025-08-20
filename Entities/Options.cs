namespace Skill_Matrix.Entities
{
	public class Options
	{
		public int Id { get; set; }
		public string OptionText { get; set; }

		public int QuizQuestionId { get; set; }
		public QuizQuestions QuizQuestion { get; set; }
	}
}
