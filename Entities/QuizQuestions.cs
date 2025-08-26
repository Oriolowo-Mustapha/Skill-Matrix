namespace Skill_Matrix.Entities
{
	public class QuizQuestions
	{
		public int Id { get; set; }
		public string Question { get; set; }
		public string CorrectAnswer { get; set; }

		// Foreign key to batch
		public int QuizBatchId { get; set; }
		public QuizBatch QuizBatch { get; set; }

		public List<Options> Options { get; set; } = new();
		public List<WrongAnswers> WrongAnswers { get; set; } = new();
	}

}
