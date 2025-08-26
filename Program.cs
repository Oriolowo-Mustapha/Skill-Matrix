using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Implementations.Repository;
using Skill_Matrix.Implementations.Services;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using SkillMatrix.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the DbContext with MySQL
builder.Services.AddDbContext<SkillMatrixDbContext>(options =>
	options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Configuration (for token generation)
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
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
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

// JWT Settings class
public class JwtSettings
{
	public string SecretKey { get; set; }
	public string Issuer { get; set; }
	public string Audience { get; set; }
	public int ExpiryMinutes { get; set; }
}