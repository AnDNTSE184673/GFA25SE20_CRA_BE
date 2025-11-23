using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IInvoiceService
    {
        Task<List<InvoiceView>?> GetInvoicesByCusId(Guid userId);
        Task<List<InvoiceView>?> GetInvoicesByVendorId(Guid vendorId);
        Task<List<InvoiceView>?> GetInvoices();
        Task<InvoiceView?> GetInvoiceById(Guid id);
        Task<InvoiceView?> CreateInvoice(InvoiceCreateRequest request);
        Task<InvoiceView?> UpdateInvoiceToCompleted(Guid id);
        Task<InvoiceView?> UpdateInvoice(InvoiceUpdateRequest request);
        Task<InvoiceView?> UpdateInvoiceToFailed(Guid id);
    }
}
