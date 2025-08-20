namespace Skill_Matrix.Entities
{
	public class WrongAnswers
	{
		public int Id { get; set; }
		public string AnswerText { get; set; }

		public int QuizQuestionId { get; set; }
		public QuizQuestions QuizQuestion { get; set; }
	}
}
