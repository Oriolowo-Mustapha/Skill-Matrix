using Microsoft.EntityFrameworkCore;
using Skill_Matrix.Data;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;

namespace Skill_Matrix.Implementations.Repository
{
	public class UserRepository : IUserRepository
	{
		private readonly SkillMatrixDbContext _context;

		public UserRepository(SkillMatrixDbContext context)
		{
			_context = context;
		}
		public async Task<User> GetByEmailOrUsernameAsync(string emailOrUsername)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);
		}

		public async Task<User> GetByIdAsync(Guid id)
		{
			return await _context.Users.FindAsync(id);
		}

		public async Task AddAsync(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> ExistsAsync(string email, string username)
		{
			return await _context.Users
				.AnyAsync(u => u.Email == email || u.Username == username);
		}
	}
}
