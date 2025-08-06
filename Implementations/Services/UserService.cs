using Microsoft.IdentityModel.Tokens;
using Skill_Matrix.DTOs;
using Skill_Matrix.Entities;
using Skill_Matrix.Interfaces.Repository;
using Skill_Matrix.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Skill_Matrix.Implementations.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IConfiguration _configuration;

		public UserService(IUserRepository userRepository, IConfiguration configuration)
		{
			_userRepository = userRepository;
			_configuration = configuration;
		}

		public async Task<UserProfileDto> RegisterAsync(UserDto registerDto)
		{
			if (await _userRepository.ExistsAsync(registerDto.Email, registerDto.Username))
				throw new Exception("Email or username already exists.");

			var user = new User
			{
				FirstName = registerDto.FirstName,
				LastName = registerDto.LastName,
				Username = registerDto.Username,
				Email = registerDto.Email,
				PasswordHash = HashPassword(registerDto.Password)
			};

			await _userRepository.AddAsync(user);

			return new UserProfileDto
			{
				Id = user.Id,
				Username = user.Username,
				Email = user.Email,
				CreatedAt = user.CreatedAt
			};
		}

		public async Task<User> GetUserForAuthentication(UserLoginDto loginDto)
		{
			var user = await _userRepository.GetByEmailOrUsernameAsync(loginDto.UsernameOrEmail);

			if (user == null || HashPassword(loginDto.Password) != user.PasswordHash)
			{
				return null;
			}

			return user;
		}

		public async Task<string> LoginAsync(UserLoginDto loginDto)
		{
			var user = await _userRepository.GetByEmailOrUsernameAsync(loginDto.UsernameOrEmail);

			if (user == null || HashPassword(loginDto.Password) != user.PasswordHash)
			{
				return null;
			}

			var token = GenerateJwtToken(user);
			return token;
		}

		public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
		{
			var user = await _userRepository.GetByIdAsync(userId);
			if (user == null)
				throw new Exception("User not found.");

			return new UserProfileDto
			{
				Id = user.Id,
				Username = user.Username,
				Email = user.Email,
				CreatedAt = user.CreatedAt
			};
		}

		private string GenerateJwtToken(User user)
		{
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var secretKey = jwtSettings["SecretKey"];
			var issuer = jwtSettings["Issuer"];
			var audience = jwtSettings["Audience"];
			var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]);

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
			};

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private static string HashPassword(string rawData)
		{
			using var sha256 = SHA256.Create();
			var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
			var builder = new StringBuilder();
			foreach (var b in bytes)
				builder.Append(b.ToString("x2"));
			return builder.ToString();
		}
	}
}
