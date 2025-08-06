using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;

namespace Skill_Matrix.Interfaces.Services
{
	public interface IUserService
	{
		Task<UserProfileDto> RegisterAsync(UserDto registerDto);
		Task<string> LoginAsync(UserLoginDto loginDto);
		Task<UserProfileDto> GetUserProfileAsync(Guid userId);
		Task<User> GetUserForAuthentication(UserLoginDto loginDto);
	}
}
