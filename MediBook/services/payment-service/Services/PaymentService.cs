using payment_service.Entities;
using payment_service.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace payment_service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IAppointmentHttpService _apptHttpService;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _config;

        public PaymentService(
            IPaymentRepository repo, 
            IAppointmentHttpService apptHttpService,
            ILogger<PaymentService> logger,
            IConfiguration config)
        {
            _repo = repo;
            _apptHttpService = apptHttpService;
            _logger = logger;
            _config = config;
            
            // QuestPDF requires setting the license even for Community use
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            _logger.LogInformation("Processing payment for AppointmentId: {AppointmentId}", payment.AppointmentId);

            // Double Payment Guard
            var existing = _repo.FindByAppointmentId(payment.AppointmentId);
            if (existing != null && (existing.Status == "Paid" || existing.Status == "Pending"))
            {
                throw new InvalidOperationException($"A payment session for Appointment {payment.AppointmentId} is already in progress or completed.");
            }

            // --- PROFESSIONAL INTEGRATION: FETCH SECURE DATA ---
            var appt = await _apptHttpService.GetAppointmentDetailsAsync(payment.AppointmentId);
            if (appt == null)
            {
                throw new KeyNotFoundException($"Appointment {payment.AppointmentId} not found in the booking system.");
            }

            // Sync data from the Secure Source of Truth
            payment.PatientId = appt.PatientId;
            payment.ProviderId = appt.ProviderId;
            payment.Amount = CalculateAmount(appt.ServiceType);
            payment.Currency = "INR"; // Hardcoded for this region

            // Mocking SDK logic (Stripe/Razorpay)
            payment.Status = "Paid";
            payment.TransactionId = "TXN_" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            _repo.Add(payment);
            _repo.SaveChanges();

            // Notify Appointment Service that payment is complete
            await _apptHttpService.UpdateAppointmentStatusAsync(payment.AppointmentId, "Completed");

            return payment;
        }

        public async Task<string> CreateRazorpayOrderAsync(int appointmentId)
        {
            _logger.LogInformation("Creating Razorpay Order for AppointmentId: {AppointmentId}", appointmentId);

            var existing = _repo.FindByAppointmentId(appointmentId);
            if (existing != null && (existing.Status == "Paid" || existing.Status == "Pending"))
            {
                throw new InvalidOperationException($"A payment session for Appointment {appointmentId} is already in progress or completed.");
            }

            var appt = await _apptHttpService.GetAppointmentDetailsAsync(appointmentId);
            if (appt == null)
            {
                throw new KeyNotFoundException($"Appointment {appointmentId} not found in the booking system.");
            }

            decimal amount = CalculateAmount(appt.ServiceType);
            
            // Initialize Razorpay Client
            string keyId = _config["Razorpay:KeyId"] ?? throw new InvalidOperationException("Razorpay KeyId is missing");
            string keySecret = _config["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret is missing");
            var client = new Razorpay.Api.RazorpayClient(keyId, keySecret);

            // Create Order
            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) }, // Amount in paise
                { "currency", "INR" },
                { "receipt", $"rcpt_appt_{appointmentId}" }
            };

            Razorpay.Api.Order order = client.Order.Create(options);
            return order["id"].ToString();
        }

        public async Task<Payment> VerifyRazorpayPaymentAsync(int appointmentId, string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            _logger.LogInformation("Verifying Razorpay Payment for AppointmentId: {AppointmentId}", appointmentId);

            string keySecret = _config["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret is missing");

            // Verify Signature
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", razorpayOrderId },
                { "razorpay_payment_id", razorpayPaymentId },
                { "razorpay_signature", razorpaySignature }
            };

            try
            {
                Razorpay.Api.Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Payment signature verification failed.", ex);
            }

            // Signature valid, proceed to save payment
            var appt = await _apptHttpService.GetAppointmentDetailsAsync(appointmentId);
            if (appt == null) throw new KeyNotFoundException($"Appointment {appointmentId} not found.");

            var payment = new Payment
            {
                AppointmentId = appointmentId,
                PatientId = appt.PatientId,
                ProviderId = appt.ProviderId,
                Amount = CalculateAmount(appt.ServiceType),
                Currency = "INR",
                Mode = "Razorpay",
                Status = "Paid",
                TransactionId = razorpayPaymentId,
                PaidAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _repo.Add(payment);
            _repo.SaveChanges();

            await _apptHttpService.UpdateAppointmentStatusAsync(appointmentId, "Completed");

            return payment;
        }

        // Bridge method to support synchronous interface if needed, but we should prefer async
        public Payment ProcessPayment(Payment payment)
        {
             return ProcessPaymentAsync(payment).GetAwaiter().GetResult();
        }

        private decimal CalculateAmount(string serviceType)
        {
            // Simple Pricing Engine Mock
            return serviceType.ToLower() switch
            {
                "general" or "consultation" => 500,
                "specialist" or "cardiologist" or "neurologist" => 1500,
                "emergency" => 2500,
                _ => 800 // Default base fee
            };
        }

        public Payment? GetPaymentByAppointment(int appointmentId)
        {
            return _repo.FindByAppointmentId(appointmentId);
        }

        public List<Payment> GetPaymentsByPatient(int patientId)
        {
            return _repo.FindByPatientId(patientId);
        }

        public List<Payment> GetPaymentHistory()
        {
            return _repo.FindByStatus("Paid").Concat(_repo.FindByStatus("Refunded")).ToList();
        }

        public Payment RefundPayment(int paymentId)
        {
            var payment = _repo.GetById(paymentId) ?? throw new KeyNotFoundException("Payment not found.");
            
            if (payment.Status != "Paid")
                throw new InvalidOperationException("Only paid payments can be refunded.");

            _logger.LogInformation("Processing refund for Transaction: {TransactionId}", payment.TransactionId);

            // Mocking SDK refund call
            payment.Status = "Refunded";
            payment.RefundedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            _repo.Update(payment);
            _repo.SaveChanges();

            // Trigger refund in Appointment-service too if needed (reset status to scheduled/cancelled)
            _apptHttpService.UpdateAppointmentStatusAsync(payment.AppointmentId, "Cancelled").Wait();

            return payment;
        }

        public string GetPaymentStatus(int paymentId)
        {
            var payment = _repo.GetById(paymentId);
            return payment?.Status ?? "NotFound";
        }

        public void UpdatePaymentStatus(int paymentId, string status)
        {
            var payment = _repo.GetById(paymentId) ?? throw new KeyNotFoundException("Payment not found.");
            payment.Status = status;
            _repo.Update(payment);
            _repo.SaveChanges();
        }

        public byte[] GenerateInvoice(int paymentId)
        {
            var payment = _repo.GetById(paymentId) ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.Status != "Paid")
                throw new InvalidOperationException("Invoice only available for successful payments.");

            // QuestPDF Logic
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MediBook Invoice").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Date: {payment.PaidAt:dd MMM yyyy}");
                            col.Item().Text($"Invoice #: INV-{payment.PaymentId}");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("MediBook Platform").FontSize(14).SemiBold();
                            col.Item().Text("Healthcare Service Provider");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(10);

                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Description");
                                header.Cell().Element(CellStyle).AlignRight().Text("Amount");

                                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            table.Cell().Element(CellStyle).Text($"Consultation (Appt #{payment.AppointmentId})");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{payment.Currency} {payment.Amount:F2}");

                            static IContainer CellStyle(IContainer container) => container.PaddingVertical(5);
                        });

                        x.Item().AlignRight().Text($"Total Amount: {payment.Currency} {payment.Amount:F2}").FontSize(14).SemiBold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public double GetTotalRevenue()
        {
            // Sum all paid payments
            // Using repo methods to get all paid then summing
            var paidPayments = _repo.FindByStatus("Paid");
            return (double)paidPayments.Sum(p => p.Amount);
        }

        public double GetTotalRevenueByProvider(int providerId)
        {
            var providerPayments = _repo.FindByProviderId(providerId)
                .Where(p => p.Status == "Paid");
            return (double)providerPayments.Sum(p => p.Amount);
        }
    }
}
