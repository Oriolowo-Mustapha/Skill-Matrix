using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Controllers
{
	public class QuizController(IWrongAnswersRepository wrongAnswersRepository, IQuizService _quizService, ISkillService _skillService, IQuizRepository _quizRepository, ISkillRepository skillRepository, ISuggestionService suggestionService, IMemoryCache cache) : Controller
	{

		[HttpPost]
		public async Task<IActionResult> TakeQuiz(string skillName, string proficiencyLevel, int questionCount)
		{
			try
			{
				// Validate input parameters
				if (string.IsNullOrWhiteSpace(skillName))
				{
					TempData["Error"] = "Please select a skill before starting the assessment.";
					return RedirectToAction("Index", "Skill");
				}

				if (string.IsNullOrWhiteSpace(proficiencyLevel))
				{
					TempData["Error"] = "Please select your proficiency level before starting the assessment.";
					return RedirectToAction("Index", "Skill");
				}

				// Validate question count
				var validQuestionCounts = new[] { 5, 10, 15, 20 };
				if (!validQuestionCounts.Contains(questionCount))
				{
					TempData["Error"] = "Please select a valid number of questions (5, 10, 15, or 20).";
					return RedirectToAction("Index", "Skill");
				}

				var userId = GetCurrentUserId();
				try
				{
					var addskill = await _skillService.AddSkillAsync(userId, skillName, proficiencyLevel);
					var quizBatchDto = await _quizService.GetQuizQuestionsAsync(skillName, questionCount, proficiencyLevel);

					if (quizBatchDto == null || !quizBatchDto.Questions.Any())
					{
						TempData["Error"] = $"Unable to load assessment questions for {skillName}. Please try again.";
						return RedirectToAction("Index", "Skill");
					}
					ViewBag.SkillId = addskill.Id;
					ViewBag.SkillName = skillName;
					ViewBag.ProficiencyLevel = proficiencyLevel;
					ViewBag.QuestionCount = quizBatchDto.Questions.Count;
					ViewBag.BatchId = quizBatchDto.BatchId;

					return View(quizBatchDto.Questions);

				}
				catch (Exception ex)
				{
					TempData["Error"] = $"Unable to load assessment questions for {skillName}. Please try again.";
					return RedirectToAction("Index", "Skill");
				}
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load assesment questions for {skillName}. Please try again.";
				return RedirectToAction("Index", "Skill");
			}
		}

		[HttpGet]
		public async Task<IActionResult> RetakeQuiz(Guid skillId)
		{
			try
			{
				var userId = GetCurrentUserId();
				if (userId == Guid.Empty)
				{
					TempData["Error"] = "User not logged in.";
					return RedirectToAction("Login", "User");
				}

				var skill = await skillRepository.GetByIdAsync(skillId);
				if (skill == null)
				{
					TempData["Error"] = "Skill not found.";
					return RedirectToAction("Index", "Skill");
				}

				var quizBatchDto = await _quizService.CreateRetakeQuizAsync(userId, skillId);

				if (quizBatchDto == null || !quizBatchDto.Questions.Any())
				{
					TempData["Error"] = $"Unable to generate retake assessment questions for {skill.SkillName}. Please try again.";
					return RedirectToAction("Index", "Skill");
				}

				ViewBag.SkillId = skill.Id;
				ViewBag.SkillName = skill.SkillName;
				ViewBag.QuestionCount = quizBatchDto.Questions.Count;
				ViewBag.BatchId = quizBatchDto.BatchId;

				return View("TakeQuiz", quizBatchDto.Questions);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"An error occurred while generating the retake quiz: {ex.Message}";
				return RedirectToAction("Index", "Skill");
			}
		}
		[HttpPost]
		public async Task<IActionResult> SubmitQuiz(Guid skillId, int quizBatchId, List<string> answers)
		{
			try
			{
				var userId = GetCurrentUserId();
				if (userId == Guid.Empty)
				{
					return RedirectToAction("Login", "User");
				}
				var result = await _quizService.SubmitQuizAsync(userId, skillId, quizBatchId, answers);
				await suggestionService.GetSuggestionsAsync(result.Id);
				if (result == null)
				{
					TempData["Error"] = $"Unable to Submit Assessment. Please try again.";
				}
				return View("Result", result);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to Submit Assessment. Please try again.";
				// When an error occurs during submission, we should ideally reload the quiz
				// with the same questions. The cache key is based on skillId, but we need the specific batch.
				// For now, we'll just get the latest from cache, which might not be the exact batch.
				// A more robust solution would involve passing the quizBatchDto.Questions back to the view
				// or storing the full QuizBatchDto in cache with the BatchId as part of the key.
				var quiz = _quizService.GetQuizQuestionsFromCache(skillId);
				return View("TakeQuiz", quiz);
			}
		}

		[HttpGet]
		public async Task<IActionResult> ViewResult(Guid QuizId)
		{
			try
			{
				var Result = await _quizRepository.GetQuizResultById(QuizId);
				var BatchId = Result.QuizBatchId;
				var WrongAnswers = await wrongAnswersRepository.GetByBatchId(BatchId);
				var Questions = await _quizRepository.GetByBatchId(BatchId);
				                var result = new ViewResultViewModel()
				                {
				                    Id = Result.Id,
				                    SkillId = Result.Skill.Id,
				                    SkillName = Result.Skill.SkillName,
				                    DateTaken = Result.DateTaken,
				                    TotalQuestions = Questions.Count,
				                    Score = Result.Score,
				                    ProficiencyLevel = Result.ProficiencyLevel,
				                    NoOfCorrectAnswers = Result.NoOfCorrectAnswers,
				                    NoOfWrongAnswers = Result.NoOfWrongAnswers,
				                    RetakeCount = Result.RetakeCount,
				                    WrongAnswers = WrongAnswers
				                }; return View("ViewResult", result);			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to fetch result. Please try again.";
				return View();
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetAssessmentDetails(Guid id)
		{
			try
			{
				var Result = await _quizRepository.GetQuizResultById(id);
				if (Result == null)
				{
					return NotFound("Assessment result not found.");
				}

				// Ensure Skill is loaded before accessing its properties
				if (Result.Skill == null)
				{
					// This should ideally not happen if Include(q => q.Skill) works, but as a safeguard
					return StatusCode(500, "Associated skill data not found for this assessment.");
				}

				var BatchId = Result.QuizBatchId;
				var WrongAnswers = await wrongAnswersRepository.GetByBatchId(BatchId);
				var Questions = await _quizRepository.GetByBatchId(BatchId);

				var result = new ViewResultViewModel()
				{
					SkillId = Result.Skill.Id,
					SkillName = Result.Skill.SkillName,
					DateTaken = Result.DateTaken,
					TotalQuestions = Questions.Count,
					Score = Result.Score,
					ProficiencyLevel = Result.ProficiencyLevel,
					NoOfCorrectAnswers = Result.NoOfCorrectAnswers,
					NoOfWrongAnswers = Result.NoOfWrongAnswers,
					RetakeCount = Result.RetakeCount,
					WrongAnswers = WrongAnswers
				};

				return PartialView("AssessmentModal", result);
			}
			catch (Exception ex)
			{
				// Log the exception for debugging purposes
				Console.Error.WriteLine($"Error in GetAssessmentDetails: {ex.Message}\n{ex.StackTrace}");
				return StatusCode(500, "An error occurred while fetching assessment details.");
			}
		}
		[HttpGet]
		public IActionResult ContinueLearning(Guid QuizResultId)
		{
			try
			{
				var Suggestion = suggestionService.GetSuggestionsFromCache(QuizResultId);
				return View("Suggestion", Suggestion);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Unable To Fetch Suggestions. Pls Try again";
				return View();
			}
		}

		[HttpGet]
		private Guid GetCurrentUserId()
		{
			var userIdSession = HttpContext.Session.GetString("UserId");

			if (string.IsNullOrEmpty(userIdSession))
			{
				return Guid.Empty;
			}

			if (Guid.TryParse(userIdSession, out Guid userId))
			{
				return userId;
			}

			return Guid.Empty;
		}
	}
}