using PayOS.Models.V2.PaymentRequests;
using Repository.DTO.RequestDTO.Payment;
using Repository.DTO.ResponseDTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IPaymentService
    {
        Task<(long, string)> CreatePayOSPaymentRequest(CreatePaymentRequest request);
        Task<(long, string)> CreatePayOSFromBooking(Guid bookingId);
        // Task<(long, string)> GetPayOSPaymentResponse(long id);
        Task<(long, string)> CreatePayOSPaymentRequestForRentalAfterBooking(Guid bookingId, Guid payId);
        Task<PaymentLink> GetPayOSPaymentResponse(long id);
        Task<List<PaymentHistoryView>?> GetHistoryForUserPayOS(Guid id);
        Task<List<PaymentHistoryView>?> GetAllPaymentPayOS();
        Task<List<PaymentHistoryView>?> GetHistoryForUser(Guid userId); 
        Task<PaymentHistoryView?> GetPaymentByOrderCode(long orderCode);
        Task<PaymentHistoryView?> GetPaymentById(Guid paymentId);
        Task<List<PaymentHistoryView>?> GetPaymentsByInvoiceId(Guid invoiceId);
        Task<List<PaymentHistoryView>?> GetAllPayment();
        Task<PaymentHistoryView?> UpdatePaymentStatusAndMethod(Guid paymentId, string status, string method);
        Task<PaymentHistoryView?> UpdatePaymentUsingOrderCode(long orderCode, string status, string method);
        Task<List<PaymentHistoryView>?> CreatePaymentFromInvoice(Guid InvoiceId);
        Task<PaymentHistoryView?> CreateNewFinePaymentFromInvoice(Guid InvoiceId, double fine);
    }
}
