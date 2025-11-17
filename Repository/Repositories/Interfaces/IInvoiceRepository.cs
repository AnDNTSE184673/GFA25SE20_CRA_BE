using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<Invoice?> GetInvoiceById(Guid invoiceId);
        Task<List<Invoice>?> GetInvoiceByCusId(Guid userId);
        Task<List<Invoice>?> GetInvoiceByVendorId(Guid userId);
        Task<List<Invoice>?> GetAllInvoices();
        Task<Invoice> CreateInvoice(InvoiceCreateRequest request);
        Task<Invoice> UpdateInvoice(InvoiceUpdateRequest request);
    }
}
