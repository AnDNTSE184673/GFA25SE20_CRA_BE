using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<PaymentHistory>
    {
        Task<PaymentHistory?> CreateNewPaymentForBookingFee(Guid invoiceId);
        Task<PaymentHistory?> CreateNewPaymentForRentalFee(Guid invoiceId);
        Task<PaymentHistory?> CreateNewPaymentForFineFee(Guid invoiceId, double fine);
        Task<PaymentHistory?> UpdatePaymentStatusAndMethod(Guid paymentId, string status, string method);
        Task<List<PaymentHistory>?> GetPaymentsByUserId(Guid userId);
        Task<PaymentHistory?> GetPaymentByOrderCode(long orderCode);
    }
}
