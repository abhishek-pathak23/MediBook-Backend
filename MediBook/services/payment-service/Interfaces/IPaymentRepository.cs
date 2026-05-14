using payment_service.Entities;

namespace payment_service.Interfaces
{
    public interface IPaymentRepository
    {
        Payment? FindByAppointmentId(int appointmentId);
        List<Payment> FindByPatientId(int patientId);
        List<Payment> FindByStatus(string status);
        Payment? FindByTransactionId(string transactionId);
        List<Payment> FindByProviderId(int providerId);
        decimal SumAmountByPatientId(int patientId);
        List<Payment> FindByPaidAtBetween(DateTime start, DateTime end);
        
        Payment? GetById(int paymentId);
        void Add(Payment payment);
        void Update(Payment payment);
        bool SaveChanges();
    }
}
