using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IInvoiceService
    {
        Task<List<Invoice>?> GetInvoicesByCusId(Guid userId);
        Task<List<Invoice>?> GetInvoicesByVendorId(Guid vendorId);
        Task<List<Invoice>?> GetInvoices();
        Task<Invoice?> CreateInvoice(InvoiceCreateRequest request);
        Task<Invoice?> UpdateInvoiceToCompleted(Guid id);
        Task<Invoice?> UpdateInvoice(InvoiceUpdateRequest request);
        Task<Invoice?> UpdateInvoiceToFailed(Guid id);
    }
}
