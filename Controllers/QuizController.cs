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

		[HttpGet]
		public async Task<IActionResult> TakeQuiz(string skillName)
		{
			try
			{
				var questions = await _quizService.GetQuizQuestionsAsync(skillName, 5);
				if (questions == null)
				{
					TempData["Error"] = $"Unable to getting quiz. Please try again.";
					RedirectToAction("Skill", "GetAllSkills");
				}
				ViewBag.SkillName = skillName;
				return View(questions);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Quiz. Please try again.\n{ex}";
				return RedirectToAction("Login", "User");
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
				return RedirectToAction("Login", "User");
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