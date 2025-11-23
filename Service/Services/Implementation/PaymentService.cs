using Microsoft.Extensions.Configuration;
using Repository.Base;
using Repository.DTO.RequestDTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using Repository.DTO.ResponseDTO.Payment;
using Repository.Data.Entities;
using AutoMapper;


namespace Service.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public PaymentService(UnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _config = configuration;
            _mapper = mapper;
        }

        public async Task<(long, string)> CreatePayOSPaymentRequest(CreatePaymentRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var configSection = _config.GetSection("PayOS");
                PayOSClient payOS = new PayOSClient(configSection["ClientId"], configSection["ApiKey"], configSection["CheckSumKey"]);
                var paymentHis = await _unitOfWork._paymentRepo.GetByIdAsync(request.PaymentId);
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = paymentHis.OrderCode,
                    Amount = (long)(request.Amount),
                    Description = $"Thanh toán cho {paymentHis.OrderCode}",
                    ReturnUrl = configSection["ReturnUrl"],
                    CancelUrl = configSection["CancelUrl"],
                    ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(request.TimeToPay).ToUnixTimeSeconds(),
                    Signature = GenerateSignature(
                        amount: ((long)(request.Amount)).ToString(),
                        cancelUrl: configSection["CancelUrl"],
                        description: $"Thanh toán cho {paymentHis.OrderCode}",
                        orderCode: paymentHis.OrderCode.ToString(),
                        returnUrl: configSection["ReturnUrl"],
                        //returnUrl: AppDomain.CurrentDomain.BaseDirectory + "payment-return",
                        checksumKey: configSection["CheckSumKey"]
                    )
                };
                paymentHis.Status = "Pending";
                paymentHis.Signature = paymentRequest.Signature;
                paymentHis.PaymentMethod = "PayOS";
                await _unitOfWork._paymentRepo.UpdateAsync(paymentHis);
                
                CreatePaymentLinkResponse response = await payOS.PaymentRequests.CreateAsync(paymentRequest);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return (response.OrderCode, response.CheckoutUrl);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(long, string)> CreatePayOSFromBooking(Guid bookingId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var configSection = _config.GetSection("PayOS");
                PayOSClient payOS = new PayOSClient(configSection["ClientId"], configSection["ApiKey"], configSection["CheckSumKey"]);
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(booking.InvoiceId);
                var payHis = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(invoice.Id);
                var bookingPayHis = payHis.Where(p => p.Item == "Booking Fee").FirstOrDefault();
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = bookingPayHis.OrderCode,
                    Amount = (long)(bookingPayHis.PaidAmount),
                    Description = $"Thanh toán cho {bookingPayHis.OrderCode}",
                    ReturnUrl = configSection["ReturnUrl"],
                    CancelUrl = configSection["CancelUrl"],
                    ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
                    Signature = GenerateSignature(
                        amount: ((long)(bookingPayHis.PaidAmount)).ToString(),
                        cancelUrl: configSection["CancelUrl"],
                        description: $"Thanh toán cho {bookingPayHis.OrderCode}",
                        orderCode: bookingPayHis.OrderCode.ToString(),
                        returnUrl: configSection["ReturnUrl"],
                        //returnUrl: AppDomain.CurrentDomain.BaseDirectory + "payment-return",
                        checksumKey: configSection["CheckSumKey"]
                    )
                };
                bookingPayHis.Status = "Pending";
                bookingPayHis.Signature = paymentRequest.Signature;
                bookingPayHis.PaymentMethod = "PayOS";
                await _unitOfWork._paymentRepo.UpdateAsync(bookingPayHis);                
                CreatePaymentLinkResponse response = await payOS.PaymentRequests.CreateAsync(paymentRequest);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveChangesAsync();
                return (response.OrderCode, response.CheckoutUrl);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaymentLink> GetPayOSPaymentResponse(long id)
        {
            var configSection = _config.GetSection("PayOS");
            PayOSClient payOS = new PayOSClient(configSection["ClientId"], configSection["ApiKey"], configSection["CheckSumKey"]);
            var response = await payOS.PaymentRequests.GetAsync(id);
            var paymentHis = await _unitOfWork._paymentRepo.GetPaymentByOrderCode(id);
            if (paymentHis != null)
            {
                paymentHis.Status = response.Status.ToString();
                await _unitOfWork._paymentRepo.UpdateAsync(paymentHis);
                await _unitOfWork.SaveChangesAsync();
            }
            return response;
        }

        public static string GenerateSignature(string amount, string cancelUrl, string description, string orderCode, string returnUrl, string checksumKey)
        {
            string rawData =
                $"amount={amount}&" +
                $"cancelUrl={cancelUrl}&" +
                $"description={description}&" +
                $"orderCode={orderCode}&" +
                $"returnUrl={returnUrl}";

            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public async Task<List<PaymentHistoryView>?> GetHistoryForUser(Guid userId)
        {
            List<PaymentHistory>? paymentHistories = await  _unitOfWork._paymentRepo.GetPaymentsByUserId(userId);
            if (paymentHistories == null || paymentHistories.Count == 0)
            {
                return null;
            }
            List<PaymentHistoryView> paymentHistoryViews = _mapper.Map<List<PaymentHistoryView>>(paymentHistories);
            return paymentHistoryViews;
        }

        public async Task<PaymentHistoryView?> GetPaymentByOrderCode(long orderCode)
        {
            var payment = await _unitOfWork._paymentRepo.GetPaymentByOrderCode(orderCode);
            if (payment == null) return null;
            var paymentView = _mapper.Map<PaymentHistoryView>(payment);
            return paymentView;
        }

        public async Task<PaymentHistoryView?> GetPaymentById(Guid paymentId)
        {
            var paymentTask = await _unitOfWork._paymentRepo.GetByIdAsync(paymentId);
            if (paymentTask == null) return null;
            var paymentView = _mapper.Map<PaymentHistoryView>(paymentTask);
            return paymentView;
        }

        public async Task<PaymentHistoryView?> UpdatePaymentStatusAndMethod(Guid paymentId, string status, string method)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var payment = await _unitOfWork._paymentRepo.GetByIdAsync(paymentId);
                if (payment == null) return null;
                var updated = await _unitOfWork._paymentRepo.UpdatePaymentStatusAndMethod(paymentId, status, method);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentView = _mapper.Map<PaymentHistoryView>(updated);
                return paymentView;
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<PaymentHistoryView?> UpdatePaymentUsingOrderCode(long orderCode, string status, string method)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var paymentTask = await _unitOfWork._paymentRepo.GetPaymentByOrderCode(orderCode);
                if (paymentTask == null) return null;
                var updated = await _unitOfWork._paymentRepo.UpdatePaymentStatusAndMethod(paymentTask.Id, status, method);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentView = _mapper.Map<PaymentHistoryView>(updated);
                return paymentView;
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<List<PaymentHistoryView>?> CreatePaymentFromInvoice(Guid InvoiceId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(InvoiceId);
                if (invoice == null) return null;
                var listPaymentHistories = new List<PaymentHistory>();
                var bookingPayment = await _unitOfWork._paymentRepo.CreateNewPaymentForBookingFee(invoice.Id);
                var rentalPayment = await _unitOfWork._paymentRepo.CreateNewPaymentForRentalFee(invoice.Id);
                listPaymentHistories.Add(bookingPayment);
                listPaymentHistories.Add(rentalPayment);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentHistoryViews = _mapper.Map<List<PaymentHistoryView>>(listPaymentHistories);
                return paymentHistoryViews;
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<PaymentHistoryView?> CreateNewFinePaymentFromInvoice(Guid InvoiceId, double fine)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(InvoiceId);
                if (invoice == null) return null;
                var finePayment = await _unitOfWork._paymentRepo.CreateNewPaymentForFineFee(invoice.Id, fine);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentHistoryView = _mapper.Map<PaymentHistoryView>(finePayment);
                return paymentHistoryView;
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<List<PaymentHistoryView>?> GetAllPayment()
        {
            var payments = await _unitOfWork._paymentRepo.GetAllAsync();
            if (payments == null || payments.Count() == 0)
            {
                return null;
            }
            var paymentViews = _mapper.Map<List<PaymentHistoryView>>(payments);
            return paymentViews;
        }
    }
}
