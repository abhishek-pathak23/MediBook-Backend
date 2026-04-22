using auth_service.Entities;

namespace auth_service.Interfaces;

public interface IAuthService
{
    Task<User> Register(User user, string plainPassword);
    Task<string> Login(string email, string password);
    Task Logout(string token);
    Task<bool> ValidateToken(string token);
    Task<string> RefreshToken(string token);
    Task<User> GetUserByEmail(string email);
    Task<User> GetUserById(int id);
    Task<List<User>> GetAllUsers();
    Task<User> UpdateProfile(int id, User user);
    Task ChangePassword(int id, string newPassword);
    Task DeactivateAccount(int id);
}
