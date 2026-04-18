using auth_service.Entities;
using auth_service.Helpers;
using auth_service.Interfaces;

namespace auth_service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<User> Register(User user, string plainPassword)
    {
        if (await _userRepository.ExistsByEmail(user.Email))
            throw new Exception("User with this email already exists.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        return await _userRepository.Add(user);
    }

    public async Task<string> Login(string email, string password)
    {
        var user = await _userRepository.FindByEmail(email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        if (!user.IsActive)
            throw new Exception("Account is deactivated.");

        return JwtHelper.GenerateJwtToken(user, _configuration);
    }

    public async Task Logout(string token)
    {
        // Mock logout - in production we would add this token to a Revoked Tokens table
        await Task.CompletedTask;
    }

    public Task<bool> ValidateToken(string token)
    {
        return Task.FromResult(!string.IsNullOrEmpty(token));
    }

    public async Task<string> RefreshToken(string token)
    {
        // Mocking refresh logic
        return await Task.FromResult(token); 
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var user = await _userRepository.FindByEmail(email);
        if (user == null) throw new Exception("User not found.");
        return user;
    }

    public async Task<User> GetUserById(int id)
    {
        var user = await _userRepository.FindByUserId(id);
        if (user == null) throw new Exception("User not found.");
        return user;
    }

    public async Task<User> UpdateProfile(int id, User updatedUser)
    {
        var user = await GetUserById(id);
        
        if (!string.IsNullOrEmpty(updatedUser.FullName)) user.FullName = updatedUser.FullName;
        if (!string.IsNullOrEmpty(updatedUser.Phone)) user.Phone = updatedUser.Phone;
        if (!string.IsNullOrEmpty(updatedUser.ProfilePicUrl)) user.ProfilePicUrl = updatedUser.ProfilePicUrl;

        await _userRepository.Update(user);
        return user;
    }

    public async Task ChangePassword(int id, string newPassword)
    {
        var user = await GetUserById(id);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.Update(user);
    }

    public async Task DeactivateAccount(int id)
    {
        var user = await GetUserById(id);
        user.IsActive = false;
        await _userRepository.Update(user);
    }
}
