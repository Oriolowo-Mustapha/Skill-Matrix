using Microsoft.AspNetCore.Mvc;
using Skill_Matrix.DTOs;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using Skill_Matrix.ViewModel;

namespace Skill_Matrix.Controllers
{
	public class UserController : Controller
	{
		private readonly IUserService _userService;
		private readonly ISkillService _skillService;
		private readonly ISkillRepository _skillRepository;
		private readonly ISuggestionRepository _suggestionRepository;
		private readonly IQuizRepository _quizRepository;

		public UserController(IUserService userService, ISkillService skillService, IQuizRepository quizRepository, ISkillRepository skillRepository, ISuggestionRepository suggestionRepository)
		{
			_userService = userService;
			_skillService = skillService;
			_quizRepository = quizRepository;
			_skillRepository = skillRepository;
			_suggestionRepository = suggestionRepository;
		}

		        [HttpGet]
		        public IActionResult Register()
		        {
		            return View();
		        }


		[HttpPost]
		public async Task<IActionResult> Register(UserDto registerDto)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _userService.RegisterAsync(registerDto);
					TempData["Message"] = "Registration successful! Please log in.";
					return RedirectToAction("Login", "User");
				}
				catch (InvalidOperationException ex)
				{
					TempData["Error"] = ex.Message;
				}
			}
			return View(registerDto);
		}

		        [HttpGet]
		        public IActionResult Login()
		        {
		            return View();
		        }
		[HttpPost]
		public async Task<IActionResult> Login(UserLoginDto loginDto, bool rememberMe)
		{
			var user = await _userService.GetUserForAuthentication(loginDto);
			if (user == null)
			{
				TempData["Error"] = "Invalid username or password.";
				return View();
			}

			// Generate JWT token
			var token = await _userService.LoginAsync(loginDto);

			// Always store in Session
			HttpContext.Session.SetString("JwtToken", token);
			HttpContext.Session.SetString("UserId", user.Id.ToString());
			HttpContext.Session.SetString("Username", user.Username);
			HttpContext.Session.SetString("UserEmail", user.Email);

			if (rememberMe)
			{
				// Store in persistent cookies too
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Secure = true, // only for HTTPS
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				};

				Response.Cookies.Append("JwtToken", token, cookieOptions);
				Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
				Response.Cookies.Append("Username", user.Username, cookieOptions);
				Response.Cookies.Append("UserEmail", user.Email, cookieOptions);
			}

			TempData["Message"] = "Login successful!";
			return RedirectToAction("Index", "Home");
		}
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			// Clear session
			HttpContext.Session.Clear();

			// Clear cookies
			Response.Cookies.Delete("JwtToken");
			Response.Cookies.Delete("UserId");
			Response.Cookies.Delete("Username");
			Response.Cookies.Delete("UserEmail");

			TempData["Message"] = "You have been logged out.";
			return RedirectToAction("Login", "User");
		}



		//[HttpGet]
		//public async Task<IActionResult> GetProfile()
		//{
		//	try
		//	{
		//		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
		//		var profile = await _userService.GetUserProfileAsync(userId);
		//		return Ok(profile);
		//	}
		//	catch (Exception ex)
		//	{
		//		return BadRequest(new { Message = ex.Message });
		//	}
		//}

		[HttpGet]
		public async Task<IActionResult> DashBoard()
		{
			var userId = GetCurrentUserId();
			if (userId == Guid.Empty)
			{
				return RedirectToAction("Login", "User");
			}
			var user = await _userService.GetUser(userId);
			var userSkill = await _skillService.GetSkillsAsync(userId);
			var totalSkill = userSkill.Count;
			var Assessments = await _quizRepository.GetResultByUserId(userId);
			var totalAssesments = Assessments.Count;
			var averageScore = await _quizRepository.GetAverageByUserId(userId);
			var Skill = await _quizRepository.GetBestAndWeakestSkillsAsync(userId);
			var bestSkill = Skill.BestSkill;
			var weakskill = Skill.WeakestSkill;
			var recentAssesment = await _quizRepository.RecentAssesment(userId);
			var skillPerformances = await _skillRepository.SkillPerformace(userId);
			var topSuggestions = await _suggestionRepository.SuggestionHightlights(userId);
			var assessmentTrends = await _quizRepository.AssessmentTrends(userId);

			var dashboard = new DashBoardViewModel
			{
				Username = user.Username,
				MemberSince = user.CreatedAt,
				TotalSkills = totalSkill,
				TotalAssessments = totalAssesments,
				AverageScore = averageScore,
				BestSkill = bestSkill,
				WeakestSkill = weakskill,
				RecentAssessments = recentAssesment,
				SkillPerformances = skillPerformances,
				TopSuggestions = topSuggestions,
				AssessmentTrends = assessmentTrends
			};

			return View(dashboard);
		}

		[HttpGet]
		public async Task<IActionResult> SkillDashBoard()
		{
			var userId = GetCurrentUserId();
			if (userId == Guid.Empty)
			{
				return RedirectToAction("Login", "User");
			}

			var userSkills = await _skillRepository.UserSkills(userId);
			return View(userSkills);
		}

		[HttpGet]
		public async Task<IActionResult> SuggestionDashBoard()
		{
			var userId = GetCurrentUserId();
			if (userId == Guid.Empty)
			{
				return RedirectToAction("Login", "User");
			}

			var userSuggestions = await _suggestionRepository.UserSuggestions(userId);
			return View(userSuggestions);
		}

		[HttpGet]
		public async Task<IActionResult> AssesmentDashBoard()
		{
			var userId = GetCurrentUserId();
			if (userId == Guid.Empty)
			{
				return RedirectToAction("Login", "User");
			}

			var userAssesments = await _quizRepository.UserAssessments(userId);
			return View(userAssesments);
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
