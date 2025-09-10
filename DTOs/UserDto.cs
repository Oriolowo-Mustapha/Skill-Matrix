using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.DTOs
{
	public class UserDto
	{
		[Required]
		[MaxLength(50)]
		public string FirstName { get; set; }

		[Required]
		[MaxLength(50)]
		public string LastName { get; set; }

		[Required]
		[MaxLength(50)]
		public string Username { get; set; }

		[Required]
		[EmailAddress]
		[MaxLength(100)]
		public string Email { get; set; }

		[Required]
		[StringLength(8)]
		public string Password { get; set; }

		[Required]
		[StringLength(8)]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

	}

	public class UserLoginDto
	{
		[Required]
		[MaxLength(100)]
		public string UsernameOrEmail { get; set; }

		[Required]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}

	public class UserProfileDto
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public DateTime CreatedAt { get; set; }
	}

}
