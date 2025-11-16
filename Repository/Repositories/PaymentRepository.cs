using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class PaymentRepository : GenericRepository<PaymentHistory>, IPaymentRepository
    {
        private readonly CRA_DbContext _context;
        public PaymentRepository(CRA_DbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaymentHistory?> CreateNewPaymentForBookingFee(Guid invoiceId)
        {
            var invoice = await _context.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == invoiceId);
            var newPayment = new PaymentHistory
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Status = "Pending",
                PaidAmount = invoice.InvoiceItems.ElementAt(1).Total,
                UserId = invoice.CustomerId,
                Item = "Booking Fee",
                PaymentMethod = "N/A",
            };
            await _context.PaymentHistories.AddAsync(newPayment);
            await _context.SaveChangesAsync();
            return  await _context.PaymentHistories.FirstOrDefaultAsync(p => p.Id == newPayment.Id);
        }

        public async Task<PaymentHistory?> CreateNewPaymentForFineFee(Guid invoiceId, double fine)
        {
            var invoice = await _context.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == invoiceId);
            var newPayment = new PaymentHistory
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Status = "Pending",
                PaidAmount = fine,
                UserId = invoice.CustomerId,
                Item = "Fine Fee",
                PaymentMethod = "N/A",
            };
            await _context.PaymentHistories.AddAsync(newPayment);
            await _context.SaveChangesAsync();
            return await _context.PaymentHistories.FirstOrDefaultAsync(p => p.Id == newPayment.Id);
        }

        public async Task<PaymentHistory?> CreateNewPaymentForRentalFee(Guid invoiceId)
        {
            var invoice = await _context.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == invoiceId);
            var newPayment = new PaymentHistory
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Status = "Pending",
                PaidAmount = invoice.InvoiceItems.ElementAt(0).Total,
                UserId = invoice.CustomerId,
                Item = "Rental Fee",
                PaymentMethod = "N/A",
            };
            await _context.PaymentHistories.AddAsync(newPayment);
            await _context.SaveChangesAsync();
            return await _context.PaymentHistories.FirstOrDefaultAsync(p => p.Id == newPayment.Id);
        }

        public async Task<PaymentHistory?> GetPaymentByOrderCode(long orderCode)
        {
            var payment = await  _context.PaymentHistories.FirstOrDefaultAsync(p => p.OrderCode == orderCode);
            if (payment == null)
            {
                return null;
            }
            return payment;
        }

        public async Task<List<PaymentHistory>?> GetPaymentsByUserId(Guid userId)
        {
            List<PaymentHistory>? payments = await  _context.PaymentHistories
                .Where(p => p.UserId == userId)
                .ToListAsync();
            return payments;
        }

        public async Task<PaymentHistory?> UpdatePaymentStatusAndMethod(Guid paymentId, string status, string method)
        {
            var payment = await  _context.PaymentHistories.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null)
            {
                return null;
            }
            if (status.IsNullOrEmpty() && method.IsNullOrEmpty())
            {
                return payment;
            }
            if (!status.IsNullOrEmpty())
            {
                payment.Status = status;
            }
            if (!method.IsNullOrEmpty())
            {
                payment.PaymentMethod = method;
            }
            payment.UpdateDate = DateTime.UtcNow;
            _context.PaymentHistories.Update(payment);
            await _context.SaveChangesAsync();
            return await _context.PaymentHistories.FirstOrDefaultAsync(p => p.Id == paymentId);
        }
    }
}
