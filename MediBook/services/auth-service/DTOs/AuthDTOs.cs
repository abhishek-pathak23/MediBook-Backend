using System.ComponentModel.DataAnnotations;

namespace auth_service.DTOs;

public class RegisterRequestDto
{
    [Required, MaxLength(150)]
    public required string FullName { get; set; }
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
    [Phone]
    public string? Phone { get; set; }
    public string Role { get; set; } = "Patient";
    public string? Provider { get; set; }
    public string? ProfilePicUrl { get; set; }
}

public class LoginRequestDto
{
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public string? RefreshToken { get; set; }
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePicUrl { get; set; }
}

public class ChangePasswordDto
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}

public class RefreshTokenRequestDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
