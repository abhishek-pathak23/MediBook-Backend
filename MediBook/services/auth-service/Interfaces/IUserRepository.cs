using auth_service.Entities;

namespace auth_service.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
    Task<User?> FindByUserId(int id);
    Task<bool> ExistsByEmail(string email);
    Task<List<User>> FindAllByRole(string role);
    Task<User?> FindByPhone(string phone);
    Task<List<User>> FindByFullNameContaining(string name);
    Task DeleteByUserId(int id);

    Task<User> Add(User user);
    Task Update(User user);
}
