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
            var carRate = await _context.CarRentalRates.Where(r => r.CarId == request.CarId)
                .FirstOrDefaultAsync();
            var rentalTotal = (carRate.DailyRate * request.RentTime)/10;
            if (user == null || carRate == null)
            {
                rentalTotal = request.CarRate * request.RentTime;
            }
            if (request.IsHoliday == true) rentalTotal = (rentalTotal * 0.2m) + rentalTotal;
            var distanceFee = 0.0m;
            if (request.DistanceInM > 0)
            {
                var kilometers = Convert.ToDecimal(request.DistanceInM) / 1000m;
                distanceFee = (kilometers * 20000m)/10;
            }
            var newInvoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNo = "INV-" + DateTime.UtcNow.AddHours(7),
                IssueDate = DateTime.UtcNow,
                DueDate = request.InvoiceDue,
                SubTotal = (request.CarRate * request.RentTime) + request.Fees,
                GrandTotal = rentalTotal ?? 0m,
                Note = request.RentType,
                CreateDate = DateTime.UtcNow,
                Status = ConstantEnum.Statuses.PENDING,
                CustomerId = request.CustomerId,
                VendorId = user?.Owner?.Id ?? Guid.Empty,
                InvoiceItems = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Item = "Car Rental After returned",
                        Description = $"Rental for Car ID: {request.CarId} - {request.RentType}",
                        Quantity = request.RentTime,
                        UnitPrice = request.CarRate,
                        Note = $"{request.RentType} rental rate after fee",
                        Total = (decimal)(rentalTotal - (rentalTotal * 0.15m))
                    },
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Item = "Booking Fees",
                        Description = "Booking Fees",
                        Note = $"{request.Fees} % of Car Rental Total With/Without Transfer Fee",
                        Quantity = 1,
                        UnitPrice = (decimal)(rentalTotal * 0.15m),
                        Total = (decimal)((rentalTotal * 0.15m) + distanceFee)
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

        public async Task<Invoice> AddNewInvoiceItem(Guid invoiceId, InvoiceItem newItem)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null)
            {
                throw new Exception("Invoice not found");
            }
            var InvoiceItem = newItem;
            InvoiceItem.InvoiceId = invoiceId;
            _context.InvoiceItems.Add(InvoiceItem);
            invoice.InvoiceItems.Add(InvoiceItem);
            invoice.GrandTotal += InvoiceItem.Total;
            _context.Update(invoice);
            await _context.SaveChangesAsync();
            var updatedInvoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
            return invoice;
        }
    }
}
