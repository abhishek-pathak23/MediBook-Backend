using review_service.Entities;

namespace review_service.Interfaces
{
    public interface IReviewRepository
    {
        Review Add(Review review);
        List<Review> FindByProviderId(int providerId);
        List<Review> FindByPatientId(int patientId);
        Review? FindByAppointmentId(int appointmentId);
        Review? GetById(int id);
        void Update(Review review);
        void Delete(int id);
        double GetAverageRating(int providerId);
        int GetTotalCount(int providerId);
        bool ExistsByAppointmentId(int appointmentId);
        List<Review> GetAll();
        bool SaveChanges();
    }
}
