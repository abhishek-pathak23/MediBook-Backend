using review_service.DTOs;
using review_service.Entities;

namespace review_service.Interfaces
{
    public interface IReviewService
    {
        Task<Review> AddReviewAsync(Review review);
        List<ReviewResponseDto> GetByProvider(int providerId);
        List<ReviewResponseDto> GetByPatient(int patientId);
        ReviewResponseDto? GetByAppointment(int appointmentId);
        Task<Review> UpdateReviewAsync(int id, Review review);
        Task DeleteReviewAsync(int id);
        double GetAvgRating(int providerId);
        int GetReviewCount(int providerId);
        List<ReviewResponseDto> GetAllReviews();
    }
}
