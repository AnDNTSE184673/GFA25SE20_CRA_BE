using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("/PayOSPayment/{orderCode}")]
        public async Task<IActionResult> GetPayOSPaymentDetails(long orderCode)
        {
            var paymentDetails = await _paymentService.GetPayOSPaymentResponse(orderCode);
            if (paymentDetails != null)
            {
                return Ok(paymentDetails);
            }
            return NotFound("No payment details found for the specified order code.");
        }

        [HttpGet("/PayOS/AllPayments")]
        public async Task<IActionResult> GetAllPayOSPayments()
        {
            var payments = await _paymentService.GetAllPaymentPayOS();
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No PayOS payments found.");
        }

        [HttpGet("/Payment/{OrderCode}")]
        public async Task<IActionResult> GetPaymentByOrderCode(long OrderCode)
        {
            var payment = await _paymentService.GetPaymentByOrderCode(OrderCode);
            if (payment != null)
            {
                return Ok(payment);
            }
            return NotFound("No payment found for the specified order code.");
        }

        [HttpGet("/Invoice/{invoiceId}")]
        public async Task<IActionResult> GetPaymentsByInvoiceId(Guid invoiceId)
        {
            var payments = await _paymentService.GetPaymentsByInvoiceId(invoiceId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified invoice ID.");
        }

        [HttpGet("/Booking/{bookingId}/Payments")]
        public async Task<IActionResult> GetPaymentsByBookingId(Guid bookingId)
        {
            var payments = await _paymentService.GetPaymentsByBookingId(bookingId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified booking ID.");
        }

        [HttpGet("/Vendor/{vendorId}")]
        public async Task<IActionResult> GetPaymentsByVendorId(Guid vendorId)
        {
            if (vendorId == Guid.Empty)
            {
                return BadRequest("Invalid vendor ID.");
            }
            var payments = await _paymentService.GetByVendor(vendorId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified vendor ID.");
        }

        [HttpGet("/Car/{carId}/Payments")]
        public async Task<IActionResult> GetPaymentsByCarId(Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest("Invalid car ID.");
            var payments = await _paymentService.GetPaymentsByCarId(carId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified car ID.");
        }

        [HttpGet("/Parking/{parkingId}/Payments")]
        public async Task<IActionResult> GetPaymentsByParkingId(Guid parkingId)
        {
            if (parkingId == Guid.Empty) return BadRequest("Invalid parking ID.");
            var payments = await _paymentService.GetPaymentsByParkLot(parkingId);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified parking ID.");
        }

        [HttpGet("/CarType/{vendorId}")]
        public async Task<IActionResult> GetPaymentsByUserCarType(Guid vendorId, string carType)
        {
            var payments = await _paymentService.GetPaymentByCarTypeForUser(vendorId, carType);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified user.");
        }

        [HttpGet("/CarType/")]
        public async Task<IActionResult> GetPaymentsByCarType([FromQuery]string carType)
        {
            if (string.IsNullOrWhiteSpace(carType)) return BadRequest("Invalid car type.");
            var payments = await _paymentService.GetPaymentsByCarType(carType);
            if (payments != null && payments.Any())
            {
                return Ok(payments);
            }
            return NotFound("No payments found for the specified car type.");
        }

        [HttpPost("/CreatePayOSPaymentRequest")]
        public async Task<IActionResult> CreatePayOSPaymentRequest([FromBody] CreatePaymentRequest request)
        {
            var (orderCode, checkoutUrl) = await _paymentService.CreatePayOSPaymentRequest(request);
            return Ok(new { OrderCode = orderCode, CheckoutUrl = checkoutUrl });
        }

        [HttpPost("/PayOS/Booking/CreateRentalPayment/")]
        public async Task<IActionResult> CreatePayOSPaymentRequestForRentalAfterBooking([FromBody] CreatePaymentRentalPayOS request)
        {
            var (orderCode, checkoutUrl) = await _paymentService.CreatePayOSPaymentRequestForRentalAfterBooking(request.BookingId);
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

        [HttpPost("/CreateAdditionalPayment")]
        public async Task<IActionResult> CreateAdditionalPayment([FromBody] CreateFromBookingOtherFee request)
        {
            var payment = await _paymentService.CreateNewAddPayFromBoooking(request.BookingId, request.Description, request.Amount);
            if (payment != null)
            {
                return Ok(new
                {
                    OrderCode = payment.Value.Item1,
                    PayOSLink = payment.Value.Item2,
                    payment.Value.Item3
                });
            }
            return BadRequest("Failed to create additional payment.");
        }

        [HttpPatch("/UpdatePayment/Booking/RentalPayment")]
        public async Task<IActionResult> UpdateRentalPayWithBooking([FromBody] UpdatePayUsingBooking request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payment = await _paymentService.UpdateRentalPayWithBooking(request.BookingId, request.status);
            if (payment != null)
            {
                return Ok(payment);
            }
            return NotFound("No payment found to update for the specified booking ID.");
        }

        [HttpPatch("/UpdatePayment/Booking/BookingPayment")]
        public async Task<IActionResult> UpdateBookingPayWithBooking([FromBody] UpdatePayUsingBooking request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payment = await _paymentService.UpdateBookingPayWithBooking(request.BookingId, request.status);
            if (payment != null)
            {
                return Ok(payment);
            }
            return NotFound("No payment found to update for the specified booking ID.");
        }

        [HttpPatch("/UpdatePayment/Booking/Payment")]
        public async Task<IActionResult> UpdatePay([FromBody] UpdatePaymentRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payments = await _paymentService.UpdatePayment(request);
            if (payments != null)
            {
                return Ok(payments);
            }
            return NotFound("No payments found to update for the specified booking ID.");
        }

        [HttpPatch("/UpdatePayment/Booking/PaymentOrderCode")]
        public async Task<IActionResult> UpdatePayWithOrderCode([FromBody] PaymentUpdateWithOrderCode request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payments = await _paymentService.UpdatePaymentWithOrderCode(request);
            if (payments != null)
            {
                return Ok(payments);
            }
            return NotFound("No payments found to update for the specified booking ID.");
        }

        [HttpPatch("/UpdatePayment/Booking/WithoutBookingConfirmed")]
        public async Task<IActionResult> UpdateBookingPaymentWithoutBookingConfirmed([FromBody] PaymentUpdateWithOrderCode request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var payments = await _paymentService.UpdateBookingPaymentWithoutBookingConfirmed(request);
            if (payments != null)
            {
                return Ok(payments);
            }
            return NotFound("No payments found to update for the specified booking ID.");
        }
    }
}
