using Microsoft.AspNetCore.Mvc;
using Skill_Matrix.Attributes;
using Skill_Matrix.Interfaces.Services;
using Skill_Matrix.ViewModel;

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
		public async Task<IActionResult> Index(int pageNumber = 1)
		{
			try
			{
				int pageSize = 12; // Skills per page
				var (skillsOnPage, totalCount) = await _skillService.GetProgrammingSkillNamesAsync(pageNumber, pageSize);

				var vm = new PagedSkillsViewModel
				{
					Skills = skillsOnPage,
					CurrentPage = pageNumber,
					TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
					HasPreviousPage = pageNumber > 1,
					HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
				};

				return View(vm);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Unable to load Skills. Please try again.\n{ex.Message}";
				return RedirectToAction("Index", "Home");
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

		[CustomAuthorize]
		[HttpGet]
		public async Task<IActionResult> GetAllSkills(int pageNumber = 1, string searchTerm = "")
		{
			var skills = await _skillService.GetSkillsAsync(searchTerm, pageNumber);

			var pagedModel = new PagedSkillsViewModel
			{
				Skills = skills.Items,
				CurrentPage = pageNumber,
				TotalPages = skills.TotalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < skills.TotalPages
			};

			ViewBag.SearchTerm = searchTerm;
			return View("Index", pagedModel);
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
