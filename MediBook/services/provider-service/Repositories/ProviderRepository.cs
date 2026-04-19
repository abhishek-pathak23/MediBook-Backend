using Microsoft.EntityFrameworkCore;
using provider_service.Data;
using provider_service.Entities;
using provider_service.Interfaces;

namespace provider_service.Repositories
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly ApplicationDbContext _context;

        public ProviderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CountBySpecialization(string specialization)
        {
            return _context.Providers.Count(p => p.Specialization.ToLower() == specialization.ToLower());
        }

        public Provider Create(Provider provider)
        {
            _context.Providers.Add(provider);
            _context.SaveChanges();
            return provider;
        }

        public void Delete(Provider provider)
        {
            _context.Providers.Remove(provider);
            _context.SaveChanges();
        }

        public IEnumerable<Provider> FindByClinicAddress(string address)
        {
            return _context.Providers.Where(p => p.ClinicAddress.Contains(address)).ToList();
        }

        public IEnumerable<Provider> FindByIsAvailable(bool isAvailable)
        {
            return _context.Providers.Where(p => p.IsAvailable == isAvailable).ToList();
        }

        public IEnumerable<Provider> FindByIsVerified(bool isVerified)
        {
            return _context.Providers.Where(p => p.IsVerified == isVerified).ToList();
        }

        public IEnumerable<Provider> FindBySpecialization(string specialization)
        {
            return _context.Providers.Where(p => p.Specialization.ToLower() == specialization.ToLower()).ToList();
        }

        public Provider? FindByUserId(int userId)
        {
            return _context.Providers.FirstOrDefault(p => p.UserId == userId);
        }

        public IEnumerable<Provider> GetAllProviders()
        {
            return _context.Providers.ToList();
        }

        public Provider? GetProviderById(int id)
        {
            return _context.Providers.FirstOrDefault(p => p.ProviderId == id);
        }

        public IEnumerable<Provider> SearchByNameOrSpecialization(string query)
        {
            var lowerQuery = query.ToLower();
            // Note: Since name isn't explicitly in Provider entity (likely part of User),
            // we will search by Bio or ClinicName or Specialization for now, 
            // as search across microservices requires more complex orchestration.
            return _context.Providers
                .Where(p => p.Specialization.ToLower().Contains(lowerQuery) || 
                            p.ClinicName.ToLower().Contains(lowerQuery) ||
                            p.Bio.ToLower().Contains(lowerQuery))
                .ToList();
        }

        public Provider Update(Provider provider)
        {
            _context.Providers.Update(provider);
            _context.SaveChanges();
            return provider;
        }
    }
}
