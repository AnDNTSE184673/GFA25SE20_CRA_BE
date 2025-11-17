using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.Payment;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("/User/{userId}")]
        public async Task<IActionResult> GetPaymentsByUserId(Guid userId)
        {
            var payments = await _paymentService.GetHistoryForUser(userId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified user.");
        }

        [HttpGet("/Invoice/All")]
        public async Task<IActionResult> GetAllInvoicePayments()
        {
            var payments = await _paymentService.GetAllPayment();
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No invoice payments found.");
        }

        [HttpPost("/CreatePayOSPaymentRequest")]
        public async Task<IActionResult> CreatePayOSPaymentRequest([FromBody] CreatePaymentRequest request)
        {
            var (orderCode, checkoutUrl) = await _paymentService.CreatePayOSPaymentRequest(request);
            return Ok(new { OrderCode = orderCode, CheckoutUrl = checkoutUrl });
        }

        [HttpPost("CreatePaymentFromInvoice/{invoiceId}")]
        public async Task<IActionResult> CreatePaymentFromInvoice(Guid invoiceId)
        {
            var payments = await _paymentService.CreatePaymentFromInvoice(invoiceId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments created from the specified invoice.");
        }
    }
}
