using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Entities;

namespace Skill_Matrix.Data
{
	public class SkillMatrixDbContext : DbContext
	{
		public SkillMatrixDbContext(DbContextOptions<SkillMatrixDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Skill> Skills { get; set; }
		public DbSet<QuizResult> QuizResults { get; set; }
		public DbSet<Suggestion> Suggestions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// User configuration
			modelBuilder.Entity<User>()
				.HasIndex(u => u.Email)
				.IsUnique();
			modelBuilder.Entity<User>()
				.HasIndex(u => u.Username)
				.IsUnique();

			// Skill configuration
			modelBuilder.Entity<Skill>()
				.HasOne(s => s.User)
				.WithMany(u => u.Skills)
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// QuizResult configuration
			modelBuilder.Entity<QuizResult>()
				.HasOne(qr => qr.User)
				.WithMany(u => u.QuizResults)
				.HasForeignKey(qr => qr.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<QuizResult>()
				.HasOne(qr => qr.Skill)
				.WithMany(s => s.QuizResults)
				.HasForeignKey(qr => qr.SkillId)
				.OnDelete(DeleteBehavior.Restrict);

			// Suggestion configuration
			modelBuilder.Entity<Suggestion>()
				.HasOne(s => s.User)
				.WithMany(u => u.Suggestions)
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Suggestion>()
				.HasOne(s => s.Skill)
				.WithMany(sk => sk.Suggestions)
				.HasForeignKey(s => s.SkillId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
