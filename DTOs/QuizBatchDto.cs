namespace Skill_Matrix.DTOs
{
    public class QuizBatchDto
    {
        public int BatchId { get; set; }
        public List<QuizDto> Questions { get; set; }
    }
}