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
                .Include(u => u.Owner).FirstOrDefaultAsync();   
            var newInvoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNo = "INV-" + DateTime.UtcNow,
                IssueDate = DateTime.UtcNow,
                DueDate = request.InvoiceDue,
                SubTotal = (request.CarRate * request.RentTime) + request.Fees,
                GrandTotal = (request.CarRate * request.RentTime),
                Note = request.RentType,
                CreateDate = DateTime.UtcNow,
                Status = ConstantEnum.Statuses.PENDING,
                CustomerId = request.CustomerId,
                VendorId = user.Owner.Id,
                InvoiceItems = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Item = "Car Rental Total",
                        Description = $"Rental for Car ID: {request.CarId} - {request.RentType}",
                        Quantity = request.RentTime,
                        UnitPrice = request.CarRate,
                        Note = $"{request.RentType} rental rate after fee",
                        Total = (request.CarRate * request.RentTime) - (request.CarRate * request.RentTime*request.Fees)/100
                    },
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Item = "Booking Fees",
                        Description = "Booking Fees",
                        Note = $"{request.Fees} % of Car Rental Total",
                        Quantity = 1,
                        UnitPrice = (request.CarRate * request.RentTime*request.Fees)/100,
                        Total = (request.CarRate * request.RentTime*request.Fees)/100
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

        public async Task<List<Invoice>?> GetAllInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Customer)
                .Include(i => i.Vendor)
                .ToListAsync();
            if (invoices == null || invoices.Count == 0)
            {
                return null;
            }
            return invoices;
        }
    }
}
