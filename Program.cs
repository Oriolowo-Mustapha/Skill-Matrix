using Microsoft.EntityFrameworkCore;
using Npgsql;
using Skill_Matrix.CustomMiddleware;
using Skill_Matrix.Data;
using Skill_Matrix.Implementations.Repository;
using Skill_Matrix.Implementations.Services;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using SkillMatrix.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
string BuildConnectionString(IConfiguration cfg)
{
	var host = Environment.GetEnvironmentVariable("DB_HOST");
	var db = Environment.GetEnvironmentVariable("DB_NAME");
	var user = Environment.GetEnvironmentVariable("DB_USERNAME");
	var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
	var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

	if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(db) &&
		!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
	{
		var csb = new NpgsqlConnectionStringBuilder
		{
			Host = host,
			Port = int.Parse(port),
			Username = user,
			Password = password,
			Database = db,
			SslMode = SslMode.Require
		};
		return csb.ToString();
	}
	return cfg.GetConnectionString("DefaultConnection") ?? string.Empty;
}

var connString = BuildConnectionString(builder.Configuration);

builder.Services.AddDbContext<SkillMatrixDbContext>(options =>
	options.UseNpgsql(connString));

// JWT Configuration
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);

// Add Repositories
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IWrongAnswersRepository, WrongAnswerRepository>();
builder.Services.AddScoped<ISuggestionRepository, SuggestionRepository>();

// Add Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();

builder.Services.AddHttpClient();

// Add Session
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseMiddleware<RememberMeMiddleware>();

app.Use(async (context, next) =>
{
	var token = context.Session.GetString("JwtToken");
	if (!string.IsNullOrEmpty(token))
	{
		context.Request.Headers.Add("Authorization", "Bearer " + token);
	}
	await next();
});

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class JwtSettings
{
	public string? SecretKey { get; set; }
	public string? Issuer { get; set; }
	public string? Audience { get; set; }
	public int ExpiryMinutes { get; set; }
}
