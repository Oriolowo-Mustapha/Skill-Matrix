using Microsoft.AspNetCore.Mvc;
using Skill_Matrix.Interfaces.Services;

namespace Skill_Matrix.Controllers
{
	public class QuizController : Controller
	{
		private readonly IQuizService _quizService;
		private readonly ISkillService _skillService;

		public QuizController(IQuizService quizService, ISkillService skillService)
		{
			_quizService = quizService;
			_skillService = skillService;
		}

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


				var addskill = await _skillService.AddSkillAsync(userId, skillName, proficiencyLevel);

				try
				{
					var questions = await _quizService.GetQuizQuestionsAsync(skillName, questionCount, proficiencyLevel);

					if (questions == null || !questions.Any())
					{
						TempData["Error"] = $"Unable to load quiz questions for {skillName}. Please try again.";
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
					TempData["Error"] = $"Error: {ex.Message}";
					return RedirectToAction("Index", "Skill");
				}
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Quiz. Please try again. Error: {ex.Message}";
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
				if (result == null)
				{
					TempData["Error"] = $"Unable to Submit quiz. Please try again.";
				}
				return View("Result", result);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to Submit Quiz. Please try again.\n{ex}";
				var quiz = _quizService.GetQuizQuestionsFromCache(skillId);
				return View("TakeQuiz", quiz);
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