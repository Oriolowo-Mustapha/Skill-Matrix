using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Entities;

namespace Skill_Matrix.Data
{
	public class SkillMatrixDbContext : DbContext
	{
		public SkillMatrixDbContext(DbContextOptions<SkillMatrixDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Skill> Skills { get; set; }
		public DbSet<QuizResult> QuizResults { get; set; }
		public DbSet<QuizQuestions> QuizQuestions { get; set; }
		public DbSet<Options> Options { get; set; }
		public DbSet<WrongAnswers> WrongAnswers { get; set; }
		public DbSet<Suggestion> Suggestions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Unique indexes
			modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
			modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

			// User → Skills
			modelBuilder.Entity<Skill>()
				.HasOne(s => s.User)
				.WithMany(u => u.Skills)
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// User → QuizResults
			modelBuilder.Entity<QuizResult>()
				.HasOne(qr => qr.User)
				.WithMany(u => u.QuizResults)
				.HasForeignKey(qr => qr.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Skill → QuizResults
			modelBuilder.Entity<QuizResult>()
				.HasOne(qr => qr.Skill)
				.WithMany(s => s.QuizResults)
				.HasForeignKey(qr => qr.SkillId)
				.OnDelete(DeleteBehavior.Cascade);

			// User → QuizQuestion
			modelBuilder.Entity<QuizQuestions>()
				.HasOne(qr => qr.User)
				.WithMany(u => u.QuizQuestions)
				.HasForeignKey(qr => qr.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Skill → QuizQuestion
			modelBuilder.Entity<QuizQuestions>()
				.HasOne(qr => qr.Skill)
				.WithMany(s => s.QuizQuestions)
				.HasForeignKey(qr => qr.SkillId)
				.OnDelete(DeleteBehavior.Cascade);

			// QuizQuestions → Options
			modelBuilder.Entity<Options>()
				.HasOne(o => o.QuizQuestion)
				.WithMany(q => q.Options)
				.HasForeignKey(o => o.QuizQuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			// QuizQuestions → WrongAnswers
			modelBuilder.Entity<WrongAnswers>()
				.HasOne(w => w.QuizQuestion)
				.WithMany(q => q.WrongAnswers)
				.HasForeignKey(w => w.QuizQuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Suggestions
			modelBuilder.Entity<Suggestion>()
				.HasOne(s => s.User)
				.WithMany(u => u.Suggestions)
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Suggestion>()
				.HasOne(s => s.Skill)
				.WithMany(sk => sk.Suggestions)
				.HasForeignKey(s => s.SkillId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Suggestion>()
				.HasOne(s => s.QuizResult)
				.WithMany(qr => qr.Suggestions)
				.HasForeignKey(s => s.QuizResultId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
