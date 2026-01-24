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
        Task<(long, string)> CreatePayOSPaymentRequestForRentalAfterBooking(Guid bookingId);
        Task<PaymentLink> GetPayOSPaymentResponse(long id);
        Task<List<PaymentHistoryView>?> GetHistoryForUserPayOS(Guid id);
        Task<List<PaymentHistoryView>?> GetAllPaymentPayOS();
        Task<List<PaymentHistoryView>?> GetHistoryForUser(Guid userId); 
        Task<List<PaymentHistoryView>?> GetPaymentsByCarId(Guid carId);
        Task<List<PaymentHistoryView>?> GetPaymentsByParkLot(Guid parkLotId);
        Task<PaymentHistoryView?> GetPaymentByOrderCode(long orderCode);
        Task<PaymentHistoryView?> GetPaymentById(Guid paymentId);
        Task<List<PaymentHistoryView>?> GetPaymentsByInvoiceId(Guid invoiceId);
        Task<List<PaymentHistoryView>?> GetPaymentsByBookingId(Guid bookingId);
        Task<List<PaymentHistoryView>?> GetAllPayment();
        Task<List<PaymentHistoryView>?> GetByVendor(Guid vendorId);
        Task<PaymentHistoryView?> UpdatePaymentStatusAndMethod(Guid paymentId, string status, string method);
        Task<PaymentHistoryView?> UpdatePaymentUsingOrderCode(long orderCode, string status, string method);
        Task<PaymentHistoryView?> UpdateRentalPayWithBooking(Guid bookingId, string status);
        Task<PaymentHistoryView?> UpdateBookingPayWithBooking(Guid bookingId, string status);
        Task<List<PaymentHistoryView>?> UpdateOtherPayWithBooking(Guid bookingId, string status);
        Task<List<PaymentHistoryView>?> CreatePaymentFromInvoice(Guid InvoiceId);
        Task<PaymentHistoryView?> CreateNewFinePaymentFromInvoice(Guid InvoiceId, decimal fine);
        Task<(long, string, PaymentHistoryView)?> CreateNewAddPayFromBoooking(Guid BookingId, string Desc, decimal Amount);
        Task<PaymentHistoryView?> CreateNewPayFromBooking(Guid BookingId, string Desc, decimal Amount);
        Task<PaymentHistoryView?> UpdatePayment(UpdatePaymentRequest request);
        Task<PaymentHistoryView?> UpdatePaymentWithOrderCode(PaymentUpdateWithOrderCode request);
        Task<PaymentHistoryView?> UpdateBookingPaymentWithoutBookingConfirmed(PaymentUpdateWithOrderCode request);
        Task<List<PaymentHistoryView>?> GetPaymentByCarTypeForUser(Guid vendorId, string carType);
        Task<List<PaymentHistoryView>?> GetPaymentsByCarType(string carType);
    }
}
