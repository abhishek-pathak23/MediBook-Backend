using System.ComponentModel.DataAnnotations;

namespace provider_service.Entities
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; } = string.Empty;
        [MaxLength(200)]
        public string Qualification { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Bio { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string ClinicName { get; set; } = string.Empty;
        [MaxLength(500)]
        public string ClinicAddress { get; set; } = string.Empty;
        public double AvgRating { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; } = true;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
