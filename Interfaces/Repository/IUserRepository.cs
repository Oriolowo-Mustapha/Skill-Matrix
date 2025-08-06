using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Repository
{
	public interface IUserRepository
	{
		Task<User> GetByEmailOrUsernameAsync(string emailOrUsername);
		Task<User> GetByIdAsync(Guid id);
		Task AddAsync(User user);
		Task<bool> ExistsAsync(string email, string username);
	}
}
