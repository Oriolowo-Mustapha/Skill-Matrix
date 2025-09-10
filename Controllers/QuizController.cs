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
					var questions = await _quizService.GetQuizQuestionsAsync(skillName, questionCount, proficiencyLevel);

					if (questions == null || !questions.Any())
					{
						TempData["Error"] = $"Unable to load assessment questions for {skillName}. Please try again.";
						return RedirectToAction("Index", "Skill");
					}
					ViewBag.SkillId = addskill.Id;
					ViewBag.SkillName = skillName;
					ViewBag.ProficiencyLevel = proficiencyLevel;
					ViewBag.QuestionCount = questionCount;

					return View(questions);

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

		[HttpPost]
		public async Task<IActionResult> SubmitQuiz(Guid skillId, List<string> answers)
		{
			try
			{
				var userId = GetCurrentUserId();
				if (userId == Guid.Empty)
				{
					return RedirectToAction("Login", "User");
				}
				var result = await _quizService.SubmitQuizAsync(userId, skillId, answers);
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
					SkillName = Result.Skill.SkillName,
					DateTaken = Result.DateTaken,
					TotalQuestions = Questions.Count,
					Score = Result.Score,
					ProficiencyLevel = Result.ProficiencyLevel,
					NoOfCorrectAnswers = Result.NoOfCorrectAnswers,
					NoOfWrongAnswers = Result.NoOfWrongAnswers,
					RetakeCount = Result.RetakeCount,
					QuizQuestions = Questions,
					WrongAnswers = WrongAnswers
				};
				return View("ViewResult", result);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to fetch result. Please try again.";
				return View();
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