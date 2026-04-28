using provider_service.DTOs;
using provider_service.Entities;
using provider_service.Interfaces;

namespace provider_service.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _repo;

        public ProviderService(IProviderRepository repo)
        {
            _repo = repo;
        }

        public void DeleteProvider(int id)
        {
            var provider = _repo.GetProviderById(id);
            if (provider == null) throw new Exception("Provider not found");
            _repo.Delete(provider);
        }

        public IEnumerable<Provider> GetAllProviders()
        {
            return _repo.GetAllProviders();
        }

        public Provider? GetProviderById(int id)
        {
            return _repo.GetProviderById(id);
        }

        public IEnumerable<Provider> GetBySpecialization(string specialization)
        {
            return _repo.FindBySpecialization(specialization);
        }

        public Provider RegisterProvider(ProviderRegistrationDto dto)
        {
            if (_repo.FindByUserId(dto.UserId) != null)
                throw new Exception("Provider profile already exists for this user.");

            var provider = new Provider
            {
                UserId = dto.UserId,
                Specialization = dto.Specialization,
                Qualification = dto.Qualification,
                ExperienceYears = dto.ExperienceYears,
                Bio = dto.Bio,
                ClinicName = dto.ClinicName,
                ClinicAddress = dto.ClinicAddress,
                AvgRating = 0.0,
                IsVerified = false,
                IsAvailable = true
            };

            return _repo.Create(provider);
        }

        public IEnumerable<Provider> SearchProviders(string query)
        {
            IEnumerable<Provider> results;
            if (string.IsNullOrWhiteSpace(query))
                results = _repo.GetAllProviders();
            else
                results = _repo.SearchByNameOrSpecialization(query);
            
            return results.Where(p => p.IsActive);
        }

        public void SetAvailability(int id, bool isAvailable)
        {
            var provider = _repo.GetProviderById(id);
            if (provider == null) throw new Exception("Provider not found");

            provider.IsAvailable = isAvailable;
            _repo.Update(provider);
        }

        public Provider UpdateProvider(int id, ProviderUpdateDto dto)
        {
            var provider = _repo.GetProviderById(id);
            if (provider == null) throw new Exception("Provider not found");

            provider.Specialization = dto.Specialization;
            provider.Qualification = dto.Qualification;
            provider.ExperienceYears = dto.ExperienceYears;
            provider.Bio = dto.Bio;
            provider.ClinicName = dto.ClinicName;
            provider.ClinicAddress = dto.ClinicAddress;

            return _repo.Update(provider);
        }

        public void UpdateRating(int id, double newRating)
        {
            var provider = _repo.GetProviderById(id);
            if (provider == null) throw new Exception("Provider not found");

            provider.AvgRating = newRating;
            _repo.Update(provider);
        }

        public void VerifyProvider(int id)
        {
            var provider = _repo.GetProviderById(id);
            if (provider == null) throw new Exception("Provider not found");

            provider.IsVerified = true;
            _repo.Update(provider);
        }

        public void SetProviderActiveStatus(int id, bool isActive)
        {
            var provider = _repo.GetProviderById(id);
            if (provider == null) throw new Exception("Provider not found");

            provider.IsActive = isActive;
            _repo.Update(provider);
        }
    }
}
