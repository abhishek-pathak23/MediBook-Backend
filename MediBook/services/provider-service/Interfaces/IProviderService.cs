using provider_service.DTOs;
using provider_service.Entities;

namespace provider_service.Interfaces
{
    public interface IProviderService
    {
        Provider RegisterProvider(ProviderRegistrationDto dto);
        Provider? GetProviderById(int id);
        IEnumerable<Provider> GetBySpecialization(string specialization);
        IEnumerable<Provider> SearchProviders(string query);
        Provider UpdateProvider(int id, ProviderUpdateDto dto);
        void VerifyProvider(int id);
        void SetAvailability(int id, bool isAvailable);
        void DeleteProvider(int id);
        void UpdateRating(int id, double newRating);
        IEnumerable<Provider> GetAllProviders();
    }
}
