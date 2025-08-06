using Microsoft.AspNetCore.Mvc;
using Skill_Matrix.DTOs;
using Skill_Matrix.Interfaces.Services;

namespace Skill_Matrix.Controllers
{
	public class UserController : Controller
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpGet]
		public async Task<IActionResult> Register()
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
		public async Task<IActionResult> Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(UserLoginDto loginDto)
		{
			var user = await _userService.GetUserForAuthentication(loginDto);
			if (user == null)
			{
				TempData["Error"] = "Invalid username or password.";
				return View();
			}

			// Generate JWT token and store in session or cookie
			var token = await _userService.LoginAsync(loginDto);

			// Store token in session for MVC usage
			HttpContext.Session.SetString("JwtToken", token);
			HttpContext.Session.SetString("UserId", user.Id.ToString());
			HttpContext.Session.SetString("Username", user.Username);
			HttpContext.Session.SetString("UserEmail", user.Email);

			TempData["Message"] = "Login successful!";
			return RedirectToAction("GetAllSkills", "skill");
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
	}
}
