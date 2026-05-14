using payment_service.Entities;

namespace payment_service.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(Payment payment);
        Payment ProcessPayment(Payment payment);
        Task<string> CreateRazorpayOrderAsync(int appointmentId);
        Task<Payment> VerifyRazorpayPaymentAsync(int appointmentId, string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
        Payment? GetPaymentByAppointment(int appointmentId);
        List<Payment> GetPaymentsByPatient(int patientId);
        List<Payment> GetPaymentHistory();
        Payment RefundPayment(int paymentId);
        string GetPaymentStatus(int paymentId);
        void UpdatePaymentStatus(int paymentId, string status);
        byte[] GenerateInvoice(int paymentId); // Changed to byte[] for PDF delivery
        double GetTotalRevenue(); // As per diagram (double), logic will handle conversion
        double GetTotalRevenueByProvider(int providerId); // Additional as per revenue requirement
    }
}
