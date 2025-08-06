using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Skill_Matrix.Attributes
{
	public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string[] _roles;

		public CustomAuthorizeAttribute(params string[] roles)
		{
			_roles = roles;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var userId = context.HttpContext.Session.GetString("UserId");

			// Check if user is logged in
			if (string.IsNullOrEmpty(userId))
			{
				context.Result = new RedirectToActionResult("Login", "User", null);
				return;
			}
		}
	}
}
