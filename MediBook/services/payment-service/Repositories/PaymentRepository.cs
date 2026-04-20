using payment_service.Data;
using payment_service.Entities;
using payment_service.Interfaces;

namespace payment_service.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Payment? FindByAppointmentId(int appointmentId)
        {
            return _context.Payments.FirstOrDefault(p => p.AppointmentId == appointmentId);
        }

        public List<Payment> FindByPatientId(int patientId)
        {
            return _context.Payments.Where(p => p.PatientId == patientId).ToList();
        }

        public List<Payment> FindByStatus(string status)
        {
            return _context.Payments.Where(p => p.Status == status).ToList();
        }

        public Payment? FindByTransactionId(string transactionId)
        {
            return _context.Payments.FirstOrDefault(p => p.TransactionId == transactionId);
        }

        public List<Payment> FindByProviderId(int providerId)
        {
            return _context.Payments.Where(p => p.ProviderId == providerId).ToList();
        }

        public decimal SumAmountByPatientId(int patientId)
        {
            return _context.Payments
                .Where(p => p.PatientId == patientId && p.Status == "Paid")
                .Sum(p => p.Amount);
        }

        public List<Payment> FindByPaidAtBetween(DateTime start, DateTime end)
        {
            return _context.Payments
                .Where(p => p.PaidAt >= start && p.PaidAt <= end && p.Status == "Paid")
                .ToList();
        }

        public Payment? GetById(int paymentId)
        {
            return _context.Payments.Find(paymentId);
        }

        public void Add(Payment payment)
        {
            _context.Payments.Add(payment);
        }

        public void Update(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}
