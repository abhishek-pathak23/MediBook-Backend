using review_service.DTOs;
using review_service.Entities;
using review_service.Interfaces;

namespace review_service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repo;
        private readonly IAppointmentHttpService _appointmentSvc;
        private readonly IProviderHttpService _providerSvc;
        private readonly INotificationHttpService _notifSvc;

        public ReviewService(IReviewRepository repo, IAppointmentHttpService appointmentSvc, IProviderHttpService providerSvc, INotificationHttpService notifSvc)
        {
            _repo = repo;
            _appointmentSvc = appointmentSvc;
            _providerSvc = providerSvc;
            _notifSvc = notifSvc;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            // 1. Verify Appointment Status (only Completed appointments can be reviewed)
            var appointment = await _appointmentSvc.GetAppointmentDetailsAsync(review.AppointmentId);
            
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment {review.AppointmentId} not found.");
            }

            if (appointment.Status != "Completed")
            {
                throw new InvalidOperationException("Reviews can only be submitted for completed appointments.");
            }

            // 2. Map patient/provider from the source of truth if needed, or verify them
            if (appointment.PatientId != review.PatientId || appointment.ProviderId != review.ProviderId)
            {
                throw new InvalidOperationException("Review patient/provider mismatch for this appointment.");
            }

            // 3. Mark as verified since we checked the service
            review.IsVerified = true;
            review.ReviewDate = DateTime.UtcNow;

            _repo.Add(review);
            _repo.SaveChanges();

            // Push updated average rating to provider-service
            await UpdateProviderRatingAsync(review.ProviderId);

            // Broadcast real-time dashboard update to the provider
            await _notifSvc.BroadcastDashboardEventAsync("NewReview", targetUserId: review.ProviderId, broadcastToAdmins: false);

            return review;
        }

        public List<ReviewResponseDto> GetByProvider(int providerId)
        {
            var reviews = _repo.FindByProviderId(providerId);
            return reviews.Select(MapToDto).ToList();
        }

        public List<ReviewResponseDto> GetByPatient(int patientId)
        {
            var reviews = _repo.FindByPatientId(patientId);
            return reviews.Select(MapToDto).ToList();
        }

        public ReviewResponseDto? GetByAppointment(int appointmentId)
        {
            var review = _repo.FindByAppointmentId(appointmentId);
            return review != null ? MapToDto(review) : null;
        }

        public async Task<Review> UpdateReviewAsync(int id, Review updatedReview)
        {
            var existing = _repo.GetById(id) ?? throw new KeyNotFoundException("Review not found.");
            
            existing.Rating = updatedReview.Rating;
            existing.Comment = updatedReview.Comment;
            existing.IsAnonymous = updatedReview.IsAnonymous;

            _repo.Update(existing);
            _repo.SaveChanges();
            await UpdateProviderRatingAsync(existing.ProviderId);
            return existing;
        }

        public async Task DeleteReviewAsync(int id)
        {
            var review = _repo.GetById(id);
            if (review != null)
            {
                int providerId = review.ProviderId;
                _repo.Delete(id);
                _repo.SaveChanges();
                await UpdateProviderRatingAsync(providerId);
            }
        }

        public double GetAvgRating(int providerId)
        {
            return _repo.GetAverageRating(providerId);
        }

        public int GetReviewCount(int providerId)
        {
            return _repo.GetTotalCount(providerId);
        }

        public List<ReviewResponseDto> GetAllReviews()
        {
            return _repo.GetAll().Select(MapToDto).ToList();
        }

        private ReviewResponseDto MapToDto(Review review)
        {
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                AppointmentId = review.AppointmentId,
                PatientId = review.PatientId,
                PatientName = review.IsAnonymous ? "Anonymous Patient" : $"Patient #{review.PatientId}",
                ProviderId = review.ProviderId,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                IsVerified = review.IsVerified,
                IsAnonymous = review.IsAnonymous
            };
        }

        /// <summary>
        /// Recalculates the average rating for a provider and pushes it to provider-service.
        /// </summary>
        private async Task UpdateProviderRatingAsync(int providerId)
        {
            try
            {
                var avgRating = _repo.GetAverageRating(providerId);
                await _providerSvc.UpdateProviderRatingAsync(providerId, avgRating);
            }
            catch (Exception)
            {
                // Non-critical: don't fail the review operation if rating sync fails
            }
        }
    }
}
