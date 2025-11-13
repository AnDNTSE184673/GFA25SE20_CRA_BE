using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        private readonly CRA_DbContext _context;
        public InvoiceRepository(CRA_DbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetInvoiceById(Guid invoiceId)
        {
            return await _context.Invoices
            .Include(i => i.InvoiceItems)
            .Include(i => i.Customer)
            .Include(i => i.Vendor)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<List<Invoice>?> GetInvoiceByCusId(Guid userId)
        {
            return await _context.Invoices
            .Include(i => i.InvoiceItems)
            .Include(i => i.Customer)
            .Include(i => i.Vendor)
            .Where(i => i.CustomerId == userId).ToListAsync();
        }

        public async Task<List<Invoice>?> GetInvoiceByVendorId(Guid userId)
        {
            return await _context.Invoices
            .Include(i => i.InvoiceItems)
            .Include(i => i.Customer)
            .Include(i => i.Vendor)
            .Where(i => i.VendorId == userId).ToListAsync();
        }

        public async Task<Invoice> CreateInvoice(InvoiceCreateRequest request)
        {
            var user = await _context.Cars.Where(c => c.Id == request.CarId)
                        .Select(c => c.UserId).FirstOrDefaultAsync();   
            var newInvoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNo = "INV-" + DateTime.Now.Ticks,
                IssueDate = DateTime.Now,
                DueDate = request.InvoiceDue,
                SubTotal = (request.CarRate * request.RentTime) + request.Fees,
                GrandTotal = (request.CarRate * request.RentTime) + request.Fees,
                Note = request.RentType,
                CreateDate = DateTime.Now,
                Status = ConstantEnum.Statuses.PAYMENT_PENDING,
                CustomerId = request.CustomerId,
                VendorId = user,
                InvoiceItems = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Description = $"Rental for Car ID: {request.CarId} - {request.RentType}",
                        Quantity = request.RentTime,
                        UnitPrice = request.CarRate,
                        Total = request.CarRate * request.RentTime
                    },
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Description = "Booking Fees",
                        Quantity = 1,
                        UnitPrice = request.Fees,
                        Total = request.Fees
                    }
                }
            };
            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync();
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Customer)
                .Include(i => i.Vendor)
                .FirstAsync(i => i.Id == newInvoice.Id);
        }

        public async Task<Invoice> UpdateInvoice(InvoiceUpdateRequest request)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == request.Id);
            if (invoice == null)
                {
                throw new Exception("Invoice not found");
            }
            invoice.Status = request.status.ToString();
            invoice.DueDate = request.DueDate;
            _context.SaveChanges();
            var updatedInv = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == request.Id);
            return updatedInv;
        }
    }
}
