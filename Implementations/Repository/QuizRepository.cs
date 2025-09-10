using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Implementations.Repository
{
	public class QuizRepository : IQuizRepository
	{
		private readonly SkillMatrixDbContext _context;

		public QuizRepository(SkillMatrixDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(QuizResult quizResult)
		{
			await _context.QuizResults.AddAsync(quizResult);
			await _context.SaveChangesAsync();
		}

		public async Task<List<QuizQuestions>> GetBySkillIdAsync(Guid userId, Guid skillId)
		{
			return await _context.QuizQuestions
				.Where(q => q.QuizBatch.SkillId == skillId && q.QuizBatch.UserId == userId)
				.OrderByDescending(q => q.QuizBatch.CreatedAt)
				.Include(q => q.Options)
				.Include(q => q.WrongAnswers)
				.ToListAsync();
		}

		public async Task<QuizResult> GetQuizResultById(Guid QuizResultId)
		{
			return await _context.QuizResults
				.Where(q => q.Id == QuizResultId)
				.Include(q => q.Skill)
				.FirstOrDefaultAsync();
		}

		public async Task<QuizResult> GetLatestByUserAndSkillAsync(Guid userId, Guid skillId)
		{
			return await _context.QuizResults
				.Where(r => r.QuizBatch.UserId == userId && r.QuizBatch.SkillId == skillId)
				.OrderByDescending(r => r.DateTaken)
				.Include(r => r.QuizBatch)
					.ThenInclude(b => b.Questions)
						.ThenInclude(q => q.Options)
				.Include(r => r.QuizBatch)
					.ThenInclude(b => b.Questions)
						.ThenInclude(q => q.WrongAnswers)
				.FirstOrDefaultAsync();
		}


		public async Task AddQuestionAsync(QuizBatch questions)
		{
			await _context.QuizBatches.AddRangeAsync(questions);
			await _context.SaveChangesAsync();
		}

		public async Task<List<QuizQuestions>> GetByBatchId(int BatchId)
		{
			return await _context.QuizQuestions
				.Where(q => q.QuizBatchId == BatchId)
				.Include(q => q.WrongAnswers)
				.ToListAsync();
		}

		public async Task<bool> GetResultBySkillId(Guid userId, Guid skillId)
		{
			var check = _context.QuizResults.Where(q => q.UserId == userId && q.SkillId == skillId);
			if (check == null || check.Any())
			{
				return true;
			}
			return false;
		}

		public async Task<List<QuizResult?>> GetResultByUserId(Guid userId)
		{
			return await _context.QuizResults.Where(q => q.UserId == userId).ToListAsync();
		}

		public async Task<double> GetAverageByUserId(Guid userId)
		{
			var results = await _context.QuizResults.Where(q => q.UserId == userId).ToListAsync();
			return results.Any() ? results.Average(q => q.Score) : 0.0;
		}

		public async Task<(string BestSkill, string WeakestSkill)> GetBestAndWeakestSkillsAsync(Guid userId)
		{
			var results = await _context.QuizResults
				.Include(q => q.Skill)
				.Where(q => q.UserId == userId)
				.ToListAsync();

			if (!results.Any())
			{
				return ("N/A", "N/A"); // no results yet
			}

			var skillAverages = results
				.GroupBy(r => new { r.SkillId, r.Skill.SkillName })
				.Select(g => new
				{
					SkillName = g.Key.SkillName,
					AverageScore = g.Average(r => r.Score)
				})
				.ToList();

			var bestSkill = skillAverages.OrderByDescending(s => s.AverageScore).FirstOrDefault();
			var weakestSkill = skillAverages.OrderBy(s => s.AverageScore).FirstOrDefault();

			return (bestSkill?.SkillName ?? "N/A", weakestSkill?.SkillName ?? "N/A");
		}

		public Task<QuizResult> GetWeakestQuizByUserId(Guid userId)
		{
			throw new NotImplementedException();
		}

		public async Task<List<AssessmentSummaryVm>> RecentAssesment(Guid UserId)
		{
			var recentAssessments = await _context.QuizResults
			.Include(q => q.Skill)
			.Where(q => q.UserId == UserId)
			.OrderByDescending(q => q.DateTaken)
			.Take(5)
			.Select(q => new AssessmentSummaryVm
			{
				AssessmentId = q.Id,
				SkillName = q.Skill.SkillName,
				Score = q.Score,
				Level = q.ProficiencyLevel,
				TakenOn = q.DateTaken
			})
			.ToListAsync();
			return recentAssessments;
		}

		public async Task<List<AssessmentTrendVm>> AssessmentTrends(Guid UserId)
		{
			var scoreTrends = await _context.QuizResults
			.Where(q => q.UserId == UserId)
			.OrderBy(q => q.DateTaken)
			.Select(q => new AssessmentTrendVm
			{
				Date = q.DateTaken,
				Score = q.Score
			})
			.ToListAsync();
			return scoreTrends;

		}

		public async Task<List<AssessmentViewModel>> UserAssessments(Guid userId)
		{
			var assessments = await _context.QuizResults
				.Include(q => q.Skill) // include skill to get SkillName
				.Where(q => q.UserId == userId)
				.OrderByDescending(q => q.DateTaken) // latest first
				.Select(q => new AssessmentViewModel
				{
					SkillName = q.Skill.SkillName,
					Score = q.Score,
					ProficiencyLevel = q.ProficiencyLevel,
					TakenOn = q.DateTaken
				})
				.ToListAsync();

			return assessments;
		}

	}
}
