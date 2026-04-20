using System.ComponentModel.DataAnnotations;

namespace payment_service.DTOs
{
    public class PaymentProcessDto
    {
        [Required]
        public int AppointmentId { get; set; }
        
        [Required]
        public string Mode { get; set; } = "Card"; // Card, UPI, Wallet, Cash
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO to receive data from the Appointment-Service.
    /// Used for cross-service validation.
    /// </summary>
    public class AppointmentResponseDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; } // Will be calculated by Pricing Engine if 0
    }

    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string TransactionId { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string Mode { get; set; } = null!;
        public DateTime PaidAt { get; set; }
    }
}
