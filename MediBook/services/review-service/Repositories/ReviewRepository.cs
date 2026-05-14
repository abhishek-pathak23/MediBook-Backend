using review_service.Data;
using review_service.Entities;
using review_service.Interfaces;

namespace review_service.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Review Add(Review review)
        {
            _context.Reviews.Add(review);
            return review;
        }

        public List<Review> FindByProviderId(int providerId)
        {
            return _context.Reviews.Where(r => r.ProviderId == providerId).ToList();
        }

        public List<Review> FindByPatientId(int patientId)
        {
            return _context.Reviews.Where(r => r.PatientId == patientId).ToList();
        }

        public Review? FindByAppointmentId(int appointmentId)
        {
            return _context.Reviews.FirstOrDefault(r => r.AppointmentId == appointmentId);
        }

        public Review? GetById(int id)
        {
            return _context.Reviews.Find(id);
        }

        public void Update(Review review)
        {
            _context.Reviews.Update(review);
        }

        public void Delete(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review != null) _context.Reviews.Remove(review);
        }

        public double GetAverageRating(int providerId)
        {
            var reviews = _context.Reviews.Where(r => r.ProviderId == providerId);
            if (!reviews.Any()) return 0;
            return reviews.Average(r => r.Rating);
        }

        public int GetTotalCount(int providerId)
        {
            return _context.Reviews.Count(r => r.ProviderId == providerId);
        }

        public bool ExistsByAppointmentId(int appointmentId)
        {
            return _context.Reviews.Any(r => r.AppointmentId == appointmentId);
        }

        public List<Review> GetAll()
        {
            return _context.Reviews.ToList();
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}
