using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly Service.Services.IInvoiceService _invoiceService;
        public InvoiceController(Service.Services.IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("AllInvoices")]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _invoiceService.GetInvoices();
            if (invoices != null && invoices.Count() > 0) return Ok(invoices);
            return NoContent();
        }

        [HttpGet("AllInvoicesFromCustomer/{cusId}")]
        public async Task<IActionResult> GetInvoicesFromCustomer(Guid cusId)
        {
            var invoices = await _invoiceService.GetInvoicesByCusId(cusId);
            if (invoices != null && invoices.Count() > 0) return Ok(invoices);
            return NoContent();
        }

        [HttpGet("AllInvoicesToVendor/{vendorId}")]
        public async Task<IActionResult> GetInvoicesToVendor(Guid vendorId)
        {
            var invoices = await _invoiceService.GetInvoicesByVendorId(vendorId);
            if (invoices != null && invoices.Count() > 0) return Ok(invoices);
            return NoContent();
        }

        [HttpGet("/{InvoiceId}")]
        public async Task<IActionResult> GetAnInvoice(Guid InvoiceId)
        {
            var invoice = await _invoiceService.GetInvoiceById(InvoiceId);
            if (invoice != null) return Ok(invoice);
            return NoContent();
        }

        [HttpPost("CreateInvoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var invoice = await _invoiceService.CreateInvoice(request);
            if (invoice != null) return Ok(invoice);
            return BadRequest();
        }

        [HttpPatch("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice(InvoiceUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();
            var invoice = await _invoiceService.UpdateInvoice(request);
            if (invoice != null) return Ok(invoice);
            return BadRequest();
        }

        [HttpPatch("InvoiceComplete")]
        public async Task<IActionResult> UpdateInvoiceToCompleted([FromBody]Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var invoice = _invoiceService.UpdateInvoiceToCompleted(id);
            if (invoice != null) return Ok(invoice);
            return BadRequest();
        }

        [HttpPatch("InvoiceFailed")]
        public async Task<IActionResult> UpdateInvoiceToFailed([FromBody] Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var invoice = _invoiceService.UpdateInvoiceToFailed(id);
            if (invoice != null) return Ok(invoice);
            return BadRequest();
        }
    }
}
