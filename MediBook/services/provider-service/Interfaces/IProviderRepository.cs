using provider_service.Entities;

namespace provider_service.Interfaces
{
    public interface IProviderRepository
    {
        IEnumerable<Provider> GetAllProviders();
        Provider? FindByUserId(int userId);
        Provider? GetProviderById(int id);
        IEnumerable<Provider> FindBySpecialization(string specialization);
        IEnumerable<Provider> FindByIsVerified(bool isVerified);
        IEnumerable<Provider> FindByIsAvailable(bool isAvailable);
        IEnumerable<Provider> SearchByNameOrSpecialization(string query);
        IEnumerable<Provider> FindByClinicAddress(string address);
        int CountBySpecialization(string specialization);
        
        Provider Create(Provider provider);
        Provider Update(Provider provider);
        void Delete(Provider provider);
    }
}
