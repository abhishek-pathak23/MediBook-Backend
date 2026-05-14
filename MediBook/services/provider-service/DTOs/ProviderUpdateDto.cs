using System.ComponentModel.DataAnnotations;

namespace provider_service.DTOs
{
    public class ProviderUpdateDto
    {
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
    }
}
