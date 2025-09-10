namespace Skill_Matrix.CustomMiddleware
{
	public class RememberMeMiddleware
	{
		private readonly RequestDelegate _next;

		public RememberMeMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (string.IsNullOrEmpty(context.Session.GetString("JwtToken")))
			{
				// Try restoring from cookies
				var token = context.Request.Cookies["JwtToken"];
				if (!string.IsNullOrEmpty(token))
				{
					context.Session.SetString("JwtToken", token);
					context.Session.SetString("UserId", context.Request.Cookies["UserId"] ?? "");
					context.Session.SetString("Username", context.Request.Cookies["Username"] ?? "");
					context.Session.SetString("UserEmail", context.Request.Cookies["UserEmail"] ?? "");
				}
			}

			await _next(context);
		}
	}

}
