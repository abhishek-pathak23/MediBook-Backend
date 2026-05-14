using auth_service.Entities;
using auth_service.Helpers;
using auth_service.Interfaces;

namespace auth_service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
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

    public async Task<List<User>> GetAllUsers()
    {
        return await _userRepository.GetAll();
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

    public async Task ChangePassword(int id, string oldPassword, string newPassword)
    {
        var user = await GetUserById(id);

        // Verify old password before allowing change
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            throw new Exception("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.Update(user);
    }

    public async Task DeactivateAccount(int id)
    {
        var user = await GetUserById(id);
        user.IsActive = false;
        await _userRepository.Update(user);
    }

    public async Task<User> ToggleUserStatus(int id)
    {
        var user = await GetUserById(id);

        // SECURITY: Admins cannot suspend or reactivate other Admin accounts
        if (user.Role == "Admin")
            throw new Exception("Admin accounts cannot be suspended by another Admin.");

        user.IsActive = !user.IsActive;
        await _userRepository.Update(user);

        // Sync with Provider Service if the user is a provider
        if (user.Role == "Provider")
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // We use an internal service key to authenticate the machine-to-machine call
                var providerUrl = _configuration["ServiceUrls:ProviderService"] ?? "http://localhost:5117/api/v1/Provider";
                var request = new HttpRequestMessage(HttpMethod.Put, $"{providerUrl}/{user.UserId}/status");
                request.Headers.Add("X-Internal-Service-Key", "medibook-internal-service-key-2024");
                
                // Pass the new status in body
                request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { isActive = user.IsActive }), System.Text.Encoding.UTF8, "application/json");
                
                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                // In production, log this exception. We don't want to crash the whole Auth flow if Provider-Service is down.
                Console.WriteLine($"Failed to sync Provider status: {ex.Message}");
            }
        }

        return user;
    }
}
