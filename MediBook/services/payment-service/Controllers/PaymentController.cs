using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using payment_service.DTOs;
using payment_service.Entities;
using payment_service.Interfaces;

namespace payment_service.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] PaymentProcessDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var payment = new Payment
                {
                    AppointmentId = request.AppointmentId,
                    Mode = request.Mode,
                    Notes = request.Notes
                };

                var processedPayment = await _paymentService.ProcessPaymentAsync(payment);

                return CreatedAtAction(nameof(GetByAppointment), new { appointmentId = processedPayment.AppointmentId }, processedPayment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class RazorpayCreateOrderRequest
        {
            public int AppointmentId { get; set; }
        }

        [HttpPost("razorpay/create-order")]
        public async Task<IActionResult> CreateRazorpayOrder([FromBody] RazorpayCreateOrderRequest request)
        {
            try
            {
                string orderId = await _paymentService.CreateRazorpayOrderAsync(request.AppointmentId);
                return Ok(new { razorpayOrderId = orderId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class RazorpayVerifyRequest
        {
            public int AppointmentId { get; set; }
            public string RazorpayOrderId { get; set; } = string.Empty;
            public string RazorpayPaymentId { get; set; } = string.Empty;
            public string RazorpaySignature { get; set; } = string.Empty;
        }

        [HttpPost("razorpay/verify")]
        public async Task<IActionResult> VerifyRazorpayPayment([FromBody] RazorpayVerifyRequest request)
        {
            try
            {
                var payment = await _paymentService.VerifyRazorpayPaymentAsync(
                    request.AppointmentId, 
                    request.RazorpayOrderId, 
                    request.RazorpayPaymentId, 
                    request.RazorpaySignature);

                return Ok(new { message = "Payment successful", payment });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("appointment/{appointmentId}")]
        public IActionResult GetByAppointment(int appointmentId)
        {
            var payment = _paymentService.GetPaymentByAppointment(appointmentId);
            if (payment == null)
                return NotFound(new { message = $"No payment found for appointment {appointmentId}" });

            return Ok(payment);
        }

        [HttpGet("patient/{patientId}")]
        public IActionResult GetByPatient(int patientId)
        {
            var payments = _paymentService.GetPaymentsByPatient(patientId);
            return Ok(payments);
        }

        [HttpGet("history")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetHistory()
        {
            var history = _paymentService.GetPaymentHistory();
            return Ok(history);
        }

        [HttpPost("refund/{appointmentId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Refund(int appointmentId)
        {
            try
            {
                var payment = _paymentService.GetPaymentByAppointment(appointmentId);
                if (payment == null)
                    return NotFound(new { message = "Payment record not found for this appointment." });

                var refundedPayment = _paymentService.RefundPayment(payment.PaymentId);
                return Ok(refundedPayment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status/{appointmentId}")]
        public IActionResult GetStatus(int appointmentId)
        {
            var payment = _paymentService.GetPaymentByAppointment(appointmentId);
            if (payment == null)
                return NotFound(new { message = "Payment record not found." });

            return Ok(new { status = payment.Status, transactionId = payment.TransactionId });
        }

        [HttpGet("invoice/{appointmentId}")]
        public IActionResult GenerateInvoice(int appointmentId)
        {
            try
            {
                var payment = _paymentService.GetPaymentByAppointment(appointmentId);
                if (payment == null)
                    return NotFound(new { message = "Payment record not found." });

                var pdfBytes = _paymentService.GenerateInvoice(payment.PaymentId);
                return File(pdfBytes, "application/pdf", $"Invoice_{appointmentId}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue/{providerId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetTotalRevenue(int providerId)
        {
            var revenue = _paymentService.GetTotalRevenueByProvider(providerId);
            return Ok(new { providerId, totalRevenue = revenue });
        }
        
        [HttpGet("revenue/all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllRevenue()
        {
            var revenue = _paymentService.GetTotalRevenue();
            return Ok(new { totalRevenue = revenue });
        }
    }
}
