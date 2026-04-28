using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using review_service.DTOs;
using review_service.Entities;
using review_service.Interfaces;

namespace review_service.Controllers
{
    [ApiController]
    [Route("api/v1/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] ReviewCreateDto dto)
        {
            try
            {
                var review = new Review
                {
                    AppointmentId = dto.AppointmentId,
                    PatientId = dto.PatientId,
                    ProviderId = dto.ProviderId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    IsAnonymous = dto.IsAnonymous
                };

                var result = await _reviewService.AddReviewAsync(review);
                return CreatedAtAction(nameof(GetByAppointment), new { id = result.AppointmentId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // Handle unique constraint or other errors
                if (ex.InnerException?.Message.Contains("IX_Reviews_AppointmentId") == true)
                {
                    return Conflict(new { message = "A review already exists for this appointment." });
                }
                return StatusCode(500, new { message = "An error occurred while creating the review.", detail = ex.Message });
            }
        }

        [HttpGet("provider/{id}")]
        public IActionResult GetByProvider(int id)
        {
            return Ok(_reviewService.GetByProvider(id));
        }

        [HttpGet("patient/{id}")]
        [Authorize]
        public IActionResult GetByPatient(int id)
        {
            return Ok(_reviewService.GetByPatient(id));
        }

        [HttpGet("appointment/{id}")]
        public IActionResult GetByAppointment(int id)
        {
            var review = _reviewService.GetByAppointment(id);
            if (review == null) return NotFound("No review found for this appointment.");
            return Ok(review);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewCreateDto dto)
        {
            try
            {
                var review = new Review
                {
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    IsAnonymous = dto.IsAnonymous
                };

                var result = await _reviewService.UpdateReviewAsync(id, review);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Review not found.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }

        [HttpGet("avgRating/{id}")]
        public IActionResult GetAvgRating(int id)
        {
            var avg = _reviewService.GetAvgRating(id);
            return Ok(new { providerId = id, averageRating = avg });
        }

        [HttpGet("count/{id}")]
        public IActionResult GetCount(int id)
        {
            var count = _reviewService.GetReviewCount(id);
            return Ok(new { providerId = id, totalReviews = count });
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Ok(_reviewService.GetAllReviews());
        }
    }
}
