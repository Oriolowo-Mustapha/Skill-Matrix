using Microsoft.AspNetCore.Mvc;
using Skill_Matrix.Attributes;
using Skill_Matrix.Interfaces.Services;

namespace Skill_Matrix.Controllers
{
	public class SkillController : Controller
	{
		private readonly ISkillService _skillService;

		public SkillController(ISkillService skillService)
		{
			_skillService = skillService;
		}

		[CustomAuthorize]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			try
			{
				var userId = GetCurrentUserId();
				if (userId == Guid.Empty)
				{
					return RedirectToAction("Login", "User");
				}
				var skills = await _skillService.GetSkillsAsync(userId);
				if (skills == null)
				{
					TempData["Error"] = "Skills Unavailable. \n Go add new skills.";
				}
				return View(skills);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Skills. Please try again.\n{ex}";
				return RedirectToAction("Login", "User");
			}

		}

		[HttpGet]
		public async Task<IActionResult> AddSkill()
		{
			return View();
		}

		[CustomAuthorize]
		[HttpPost]
		public async Task<IActionResult> AddSkill(string skillName)
		{
			try
			{
				var userId = GetCurrentUserId();
				if (userId == Guid.Empty)
				{
					return RedirectToAction("Login", "User");
				}
				var skill = await _skillService.AddSkillAsync(userId, skillName);
				if (skill == null)
				{
					TempData["Error"] = $"Unable to Add Skills. Please try again.";
				}
				TempData["Message"] = $"Skills Added Succesfully.";
				return RedirectToAction("TakeQuiz", "Quiz");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Skills. Please try again.\n{ex}";
				return RedirectToAction("Login", "User");
			}
		}


		//[CustomAuthorize]
		[HttpGet]
		public async Task<IActionResult> GetAllSkills()
		{
			try
			{
				//var userId = GetCurrentUserId();
				//if (userId == Guid.Empty)
				//{
				//	return RedirectToAction("Login", "User");
				//}
				var getSkills = await _skillService.GetSkillNamesOnlyAsync();
				return View(getSkills);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Skills. Please try again.\n{ex}";
				return RedirectToAction("Login", "User");
			}
		}

		[CustomAuthorize]
		[HttpDelete]
		public async Task<IActionResult> DeleteSkill(Guid id)
		{
			try
			{
				var userId = GetCurrentUserId();
				if (userId == Guid.Empty)
				{
					return RedirectToAction("Login", "User");
				}
				await _skillService.DeleteSkillAsync(userId, id);
				return RedirectToAction();
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Skills. Please try again.\n{ex}";
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
