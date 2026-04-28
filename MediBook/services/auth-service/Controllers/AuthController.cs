using auth_service.DTOs;
using auth_service.Entities;
using auth_service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var userToCreate = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            // SECURITY FIX: Public registration can only ever be Patient or Provider
            Role = !string.IsNullOrEmpty(request.Role) && request.Role == "Provider" ? "Provider" : "Patient",
            Provider = null,   // SECURITY: Cannot self-assign a provider profile
            ProfilePicUrl = request.ProfilePicUrl
        };

        var createdUser = await _authService.Register(userToCreate, request.Password);

        return Ok(new AuthResponseDto
        {
            Token = "Login next to retrieve token",
            UserId = createdUser.UserId,
            FullName = createdUser.FullName,
            Email = createdUser.Email,
            Role = createdUser.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var token = await _authService.Login(request.Email, request.Password);
            var user = await _authService.GetUserByEmail(request.Email);

            return Ok(new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }
        catch (Exception ex) when (ex.Message == "Invalid credentials." || ex.Message == "Account is deactivated.")
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("create-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] RegisterRequestDto request)
    {
        var userToCreate = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = "Admin", // FORCE ADMIN ROLE
            Provider = null,
            ProfilePicUrl = request.ProfilePicUrl
        };

        var createdUser = await _authService.Register(userToCreate, request.Password);

        return Ok(new
        {
            message = "Admin user created successfully.",
            UserId = createdUser.UserId,
            Email = createdUser.Email
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        await _authService.Logout(token);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var newToken = await _authService.RefreshToken(request.Token);
        return Ok(new { token = newToken });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        if (email == null) return Unauthorized();

        var user = await _authService.GetUserByEmail(email);
        return Ok(ToProfileDto(user));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        var updateEntity = new User
        {
            FullName = request.FullName ?? "",
            Phone = request.Phone,
            ProfilePicUrl = request.ProfilePicUrl,
            Email = ""
        };

        var updatedUser = await _authService.UpdateProfile(userId, updateEntity);
        return Ok(ToProfileDto(updatedUser));
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        try
        {
            await _authService.ChangePassword(userId, request.OldPassword, request.NewPassword);
            return Ok(new { message = "Password updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        await _authService.DeactivateAccount(userId);
        return Ok(new { message = "Account deactivated" });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsers();
        var dtos = users.Select(ToProfileDto).ToList();
        return Ok(dtos);
    }

    [HttpPut("users/{userId}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleUserStatus(int userId)
    {
        var updatedUser = await _authService.ToggleUserStatus(userId);
        return Ok(ToProfileDto(updatedUser));
    }

    private static UserProfileDto ToProfileDto(User user) => new UserProfileDto
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role,
        Provider = user.Provider,
        ProfilePicUrl = user.ProfilePicUrl,
        CreatedAt = user.CreatedAt,
        IsActive = user.IsActive
    };
}
