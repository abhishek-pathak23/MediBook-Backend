using System.ComponentModel.DataAnnotations;

namespace payment_service.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Refunded, Failed

        [Required]
        [StringLength(50)]
        public string Mode { get; set; } = "Unknown"; // Card, UPI, Wallet, Cash

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "INR";

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public DateTime? RefundedAt { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
