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
using Repository.Constant;


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
                    Description = $"{paymentHis.OrderCode}",
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
                    Description = $"{bookingPayHis.OrderCode}",
                    ReturnUrl = configSection["ReturnUrl"],
                    CancelUrl = configSection["CancelUrl"],
                    ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
                    Signature = GenerateSignature(
                        amount: ((long)(bookingPayHis.PaidAmount)).ToString(),
                        cancelUrl: configSection["CancelUrl"],
                        description: $"{bookingPayHis.OrderCode}",
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

        public async Task<PaymentLink> GetPayOSPaymentResponse(long id)
        {
            var configSection = _config.GetSection("PayOS");
            PayOSClient payOS = new PayOSClient(configSection["ClientId"], configSection["ApiKey"], configSection["CheckSumKey"]);
            var response = await payOS.PaymentRequests.GetAsync(id);
            var paymentHis = await _unitOfWork._paymentRepo.GetPaymentByOrderCode(id);
            if (paymentHis != null)
            {
                paymentHis.Status = response.Status.ToString();
                var bookings = await _unitOfWork._bookingRepo.GetBookingsFromCustomer(paymentHis.UserId);
                var bknd = bookings.FirstOrDefault(x => x.InvoiceId == paymentHis.InvoiceId);
                if (paymentHis.Status.Equals("Paid") || paymentHis.Status.Equals("PAID"))
                {
                    if (paymentHis.Item.Equals("Booking Fee"))
                    {
                        bknd.UpdateDate = DateTime.UtcNow;
                        bknd.Status = ConstantEnum.Statuses.CONFIRMED;
                        var car = await _unitOfWork._carRepo.GetByIdAsync(bknd.CarId);
                        car.Status = ConstantEnum.Statuses.RESERVED;
                    }
                    else if (paymentHis.Item.Equals("Rental Fee"))
                    {
                        bknd.UpdateDate = DateTime.UtcNow;
                        bknd.Status = ConstantEnum.Statuses.COMPLETED;
                        var car = await _unitOfWork._carRepo.GetByIdAsync(bknd.CarId);
                        car.Status = ConstantEnum.Statuses.ACTIVE;
                    }
                }
                else if (paymentHis.Status.Equals("CANCELLED") || paymentHis.Status.Equals("Cancelled") || paymentHis.Status.Equals("Expired") || paymentHis.Status.Equals("Expired"))
                {
                    bknd.UpdateDate = DateTime.UtcNow;
                    bknd.Status = ConstantEnum.Statuses.CANCELLED;
                    var car = await _unitOfWork._carRepo.GetByIdAsync(bknd.CarId);
                    car.Status = ConstantEnum.Statuses.ACTIVE;
                    await _unitOfWork._carRepo.UpdateAsync(car);
                }
                await _unitOfWork._bookingRepo.UpdateAsync(bknd);
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
            List<PaymentHistory>? paymentHistories = await _unitOfWork._paymentRepo.GetPaymentsByUserId(userId);
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

        public async Task<PaymentHistoryView?> CreateNewFinePaymentFromInvoice(Guid InvoiceId, decimal fine)
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

        public async Task<List<PaymentHistoryView>?> GetHistoryForUserPayOS(Guid id)
        {
            var paymentHistories = await _unitOfWork._paymentRepo.GetAllAsync();
            List<PaymentHistory>? paymentHistoriesPayOS = paymentHistories
                .Where(p => p.UserId == id && p.PaymentMethod == "PayOS")
                .ToList();
            if (paymentHistoriesPayOS == null || paymentHistoriesPayOS.Count == 0) return null;
            List<PaymentHistoryView> paymentHistoryViews = _mapper.Map<List<PaymentHistoryView>>(paymentHistoriesPayOS);
            return paymentHistoryViews;
        }

        public async Task<List<PaymentHistoryView>?> GetAllPaymentPayOS()
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var allPayments = await _unitOfWork._paymentRepo.GetAllAsync();
                List<PaymentHistory>? paymentHistories = allPayments
                    .Where(p => p.PaymentMethod == "PayOS")
                    .ToList();
                if (paymentHistories == null || paymentHistories.Count == 0)
                {
                    return null;
                }
                foreach (var payment in paymentHistories)
                {
                    var payOSResponse = await GetPayOSPaymentResponse(payment.OrderCode);
                    if (payment.Status != "Paid" && payment.Status != "Success" && payment.Status != "SUCCESS")
                    {
                        payment.Status = payOSResponse.Status.ToString();
                        payment.UpdateDate = DateTime.UtcNow;
                    }
                    await _unitOfWork._paymentRepo.UpdateAsync(payment);

                }
                await _unitOfWork.SaveChangesAsync();
                var updatedPayments = await _unitOfWork._paymentRepo.GetAllAsync();
                foreach (var paymentView in updatedPayments)
                {
                    var updatedPayment = await _unitOfWork._paymentRepo.GetByIdAsync(paymentView.Id);
                    var bookking = await _unitOfWork._bookingRepo.GetBookingsFromCustomer(updatedPayment.UserId);
                    foreach (var books in bookking)
                    {
                        if (books.InvoiceId == updatedPayment.InvoiceId)
                        {
                            if (updatedPayment.Status == "Cancelled" || updatedPayment.Status == "Expired")
                            {
                                books.Status = "Cancelled";
                                books.UpdateDate = DateTime.UtcNow;
                                var car = await _unitOfWork._carRepo.GetByIdAsync(books.CarId);
                                car.Status = ConstantEnum.Statuses.ACTIVE;
                                await _unitOfWork._carRepo.UpdateAsync(car);
                                await _unitOfWork._bookingRepo.UpdateAsync(books);
                            }
                            else if (updatedPayment.Status == "Paid" || updatedPayment.Status == "Success" || updatedPayment.Status == "SUCCESS")
                            {
                                books.Status = "Confirmed";
                                books.UpdateDate = DateTime.UtcNow;
                                var car = await _unitOfWork._carRepo.GetByIdAsync(books.CarId);
                                car.Status = ConstantEnum.Statuses.RESERVED;
                                await _unitOfWork._carRepo.UpdateAsync(car);
                                await _unitOfWork._bookingRepo.UpdateAsync(books);
                            }
                        }
                    }

                }
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentHistoriesUpdated = await _unitOfWork._paymentRepo.GetAllAsync();
                List<PaymentHistory>? paymentHistoriesPayOS = paymentHistoriesUpdated
                    .Where(p => p.PaymentMethod == "PayOS")
                    .ToList();
                return _mapper.Map<List<PaymentHistoryView>>(paymentHistoriesPayOS);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(long, string)> CreatePayOSPaymentRequestForRentalAfterBooking(Guid bookingId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var configSection = _config.GetSection("PayOS");
                PayOSClient payOS = new PayOSClient(configSection["ClientId"], configSection["ApiKey"], configSection["CheckSumKey"]);
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                var payFromBooking = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(booking.InvoiceId);
                var rentalPayHis = payFromBooking.Where(p => p.Item == "Rental Fee").FirstOrDefault();
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = rentalPayHis.OrderCode,
                    Amount = (long)(rentalPayHis.PaidAmount),
                    Description = $"{rentalPayHis.OrderCode}",
                    ReturnUrl = configSection["ReturnUrl"],
                    CancelUrl = configSection["CancelUrl"],
                    ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(20).ToUnixTimeSeconds(),
                    Signature = GenerateSignature(
                        amount: ((long)(rentalPayHis.PaidAmount)).ToString(),
                        cancelUrl: configSection["CancelUrl"],
                        description: $"{rentalPayHis.OrderCode}",
                        orderCode: rentalPayHis.OrderCode.ToString(),
                        returnUrl: configSection["ReturnUrl"],
                        //returnUrl: AppDomain.CurrentDomain.BaseDirectory + "payment-return",
                        checksumKey: configSection["CheckSumKey"]
                    )
                };
                rentalPayHis.Status = "Pending";
                rentalPayHis.Signature = paymentRequest.Signature;
                rentalPayHis.PaymentMethod = "PayOS";
                await _unitOfWork._paymentRepo.UpdateAsync(rentalPayHis);
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

        public async Task<List<PaymentHistoryView>?> GetPaymentsByInvoiceId(Guid invoiceId)
        {
            var payments = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(invoiceId);
            if (payments == null || payments.Count == 0)
            {
                return null;
            }
            var paymentViews = _mapper.Map<List<PaymentHistoryView>>(payments);
            return paymentViews;
        }

        public async Task<List<PaymentHistoryView>?> GetPaymentsByBookingId(Guid bookingId)
        {
            var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return null;
            }
            var payments = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(booking.InvoiceId);
            if (payments == null || payments.Count == 0)
            {
                return null;
            }
            var paymentViews = _mapper.Map<List<PaymentHistoryView>>(payments);
            return paymentViews;
        }

        public async Task<PaymentHistoryView?> UpdateRentalPayWithBooking(Guid bookingId, string status)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var bookingTask = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                var pays = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(bookingTask.InvoiceId);
                var rentalPay = pays.Where(p => p.Item == "Rental Fee").FirstOrDefault();
                if (ConstantEnum.Statuses.PAID.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    rentalPay.Status = ConstantEnum.Statuses.PAID;
                    rentalPay.PaymentMethod = "Cash On Delivery";
                    rentalPay.UpdateDate = DateTime.UtcNow;
                    bookingTask.Status = ConstantEnum.Statuses.COMPLETED;
                    bookingTask.UpdateDate = DateTime.UtcNow;
                    var car = await _unitOfWork._carRepo.GetByIdAsync(bookingTask.CarId);
                    car.Status = ConstantEnum.Statuses.ACTIVE;
                    await _unitOfWork._carRepo.UpdateAsync(car);
                    await _unitOfWork._bookingRepo.UpdateAsync(bookingTask);
                    await _unitOfWork._paymentRepo.UpdateAsync(rentalPay);
                }
                if (ConstantEnum.Statuses.CANCELLED.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    rentalPay.Status = ConstantEnum.Statuses.CANCELLED;
                    rentalPay.UpdateDate = DateTime.UtcNow;
                    bookingTask.Status = ConstantEnum.Statuses.CANCELLED;
                    bookingTask.UpdateDate = DateTime.UtcNow;
                    var car = await _unitOfWork._carRepo.GetByIdAsync(bookingTask.CarId);
                    car.Status = ConstantEnum.Statuses.ACTIVE;
                    await _unitOfWork._carRepo.UpdateAsync(car);
                    await _unitOfWork._bookingRepo.UpdateAsync(bookingTask);
                    await _unitOfWork._paymentRepo.UpdateAsync(rentalPay);
                }
                await _unitOfWork.SaveChangesAsync();
                var updatedPayment = await _unitOfWork._paymentRepo.GetByIdAsync(rentalPay.Id);
                if (updatedPayment == null) return null;
                _unitOfWork.CommitTransaction();
                var paymentView = _mapper.Map<PaymentHistoryView>(updatedPayment);
                return paymentView;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaymentHistoryView?> UpdateBookingPayWithBooking(Guid bookingId, string status)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var bookingTask = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                var pays = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(bookingTask.InvoiceId);
                var rentalPay = pays.Where(p => p.Item == "Booking Fee").FirstOrDefault();
                if (ConstantEnum.Statuses.PAID.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    rentalPay.Status = ConstantEnum.Statuses.PAID;
                    rentalPay.PaymentMethod = "Cash On Delivery";
                    rentalPay.UpdateDate = DateTime.UtcNow;
                    bookingTask.Status = ConstantEnum.Statuses.COMPLETED;
                    bookingTask.UpdateDate = DateTime.UtcNow;
                    await _unitOfWork._bookingRepo.UpdateAsync(bookingTask);
                    await _unitOfWork._paymentRepo.UpdateAsync(rentalPay);
                }
                if (ConstantEnum.Statuses.CANCELLED.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    rentalPay.Status = ConstantEnum.Statuses.CANCELLED;
                    rentalPay.UpdateDate = DateTime.UtcNow;
                    bookingTask.Status = ConstantEnum.Statuses.CANCELLED;
                    bookingTask.UpdateDate = DateTime.UtcNow;
                    var car = await _unitOfWork._carRepo.GetByIdAsync(bookingTask.CarId);
                    car.Status = ConstantEnum.Statuses.ACTIVE;
                    await _unitOfWork._carRepo.UpdateAsync(car);
                    await _unitOfWork._bookingRepo.UpdateAsync(bookingTask);
                    await _unitOfWork._paymentRepo.UpdateAsync(rentalPay);
                }
                await _unitOfWork.SaveChangesAsync();
                var updatedPayment = await _unitOfWork._paymentRepo.GetByIdAsync(rentalPay.Id);
                if (updatedPayment == null) return null;
                _unitOfWork.CommitTransaction();
                var paymentView = _mapper.Map<PaymentHistoryView>(updatedPayment);
                return paymentView;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<PaymentHistoryView>?> UpdateOtherPayWithBooking(Guid bookingId, string status)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var bookingTask = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                var pays = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(bookingTask.InvoiceId);
                var FinePays = pays.Where(p => p.Item == "Fine Fee").ToList();
                if (ConstantEnum.Statuses.PAID.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var rentalPay in FinePays)
                    {
                        rentalPay.Status = ConstantEnum.Statuses.PAID;
                        rentalPay.PaymentMethod = "Cash On Delivery";
                        bookingTask.UpdateDate = DateTime.UtcNow;
                        bookingTask.Status = ConstantEnum.Statuses.COMPLETED;
                        bookingTask.UpdateDate = DateTime.UtcNow;
                        await _unitOfWork._bookingRepo.UpdateAsync(bookingTask);
                        await _unitOfWork._paymentRepo.UpdateAsync(rentalPay);
                    }
                }
                if (ConstantEnum.Statuses.CANCELLED.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var rentalPay in FinePays)
                    {
                        rentalPay.Status = ConstantEnum.Statuses.CANCELLED;
                        rentalPay.UpdateDate = DateTime.UtcNow;
                        bookingTask.Status = ConstantEnum.Statuses.CANCELLED;
                        bookingTask.UpdateDate = DateTime.UtcNow;
                        await _unitOfWork._bookingRepo.UpdateAsync(bookingTask);
                        await _unitOfWork._paymentRepo.UpdateAsync(rentalPay);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                var updatedPayments = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(bookingTask.InvoiceId);
                if (updatedPayments == null) return null;
                _unitOfWork.CommitTransaction();
                var updatedPayment = updatedPayments.Where(p => p.Item == "Fine Fee").ToList();
                var paymentView = _mapper.Map<List<PaymentHistoryView>>(updatedPayment);
                return paymentView;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(long, string, PaymentHistoryView)?> CreateNewAddPayFromBoooking(Guid BookingId, string Desc, decimal Amount)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var configSection = _config.GetSection("PayOS");
                PayOSClient payOS = new PayOSClient(configSection["ClientId"], configSection["ApiKey"], configSection["CheckSumKey"]);
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(BookingId);
                InvoiceItem newItem = new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    Item = "Additional Payment",
                    Description = Desc,
                    Quantity = 1,
                    UnitPrice = Amount,
                    InvoiceId = booking.InvoiceId,
                    Note = "Additional Payment",
                    Total = Amount,
                };
                await _unitOfWork._invoiceRepo.AddNewInvoiceItem(booking.InvoiceId, newItem);
                await _unitOfWork.SaveChangesAsync();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(booking.InvoiceId);
                var addPayment = await _unitOfWork._paymentRepo.CreateNewPaymentForAdditionFee(invoice.Id, Amount);
                await _unitOfWork.SaveChangesAsync();
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = addPayment.OrderCode,
                    Amount = (long)(addPayment.PaidAmount),
                    Description = $"{addPayment.OrderCode}",
                    ReturnUrl = configSection["ReturnUrl"],
                    CancelUrl = configSection["CancelUrl"],
                    ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(20).ToUnixTimeSeconds(),
                    Signature = GenerateSignature(
                        amount: ((long)(addPayment.PaidAmount)).ToString(),
                        cancelUrl: configSection["CancelUrl"],
                        description: $"{addPayment.OrderCode}",
                        orderCode: addPayment.OrderCode.ToString(),
                        returnUrl: configSection["ReturnUrl"],
                        //returnUrl: AppDomain.CurrentDomain.BaseDirectory + "payment-return",
                        checksumKey: configSection["CheckSumKey"]
                    )
                };
                addPayment.Status = "Pending";
                addPayment.Signature = paymentRequest.Signature;
                addPayment.PaymentMethod = "PayOS";
                await _unitOfWork._paymentRepo.UpdateAsync(addPayment);
                CreatePaymentLinkResponse response = await payOS.PaymentRequests.CreateAsync(paymentRequest);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var updatedPayment = await _unitOfWork._paymentRepo.GetByIdAsync(addPayment.Id);
                var paymentView = _mapper.Map<PaymentHistoryView>(updatedPayment);
                return (response.OrderCode, response.CheckoutUrl, paymentView);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<PaymentHistoryView?> CreateNewPayFromBooking(Guid BookingId, string Desc, decimal Amount)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(BookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(booking.InvoiceId);
                var invoiceExte = invoice.InvoiceItems.Where(x => x.Item.Contains("Extension")).FirstOrDefault();
                if (invoiceExte == null)
                {
                    throw new Exception("No extension item found in the invoice");
                }
                var newPayment = new PaymentHistory
                {
                    Id = Guid.NewGuid(),
                    OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Item = invoiceExte.Item,
                    PaidAmount = invoiceExte.Total,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    Status = "Pending",
                    InvoiceId = invoice.Id,
                    UserId = booking.UserId,
                    PaymentProofUrl = "N/A",
                    PaymentMethod = "N/A",
                    Note = "Payment for extension booking fee",
                };
                await _unitOfWork._paymentRepo.CreateAsync(newPayment);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentView = _mapper.Map<PaymentHistoryView>(newPayment);
                return paymentView;

            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<PaymentHistoryView?> UpdatePayment(UpdatePaymentRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var payment = await _unitOfWork._paymentRepo.GetByIdAsync(request.PaymentId);
                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment with Id {request.PaymentId} not found");
                }

                var incoming = request.Status?.Trim();
                if (string.IsNullOrEmpty(incoming))
                {
                    throw new InvalidOperationException($"Invalid status value: {request.Status}");
                }

                // Define which statuses this method accepts (canonical constants)
                var allowedStatuses = new[]
                {
                    ConstantEnum.Statuses.PAID,
                    ConstantEnum.Statuses.CANCELLED
                };

                var matched = allowedStatuses
                    .FirstOrDefault(s => s.Equals(incoming, StringComparison.OrdinalIgnoreCase));

                if (matched == null)
                {
                    throw new InvalidOperationException($"Invalid status value: {request.Status}");
                }

                // Use canonical constant instead of arbitrary casing
                payment.Status = matched;

                if (ConstantEnum.Statuses.PAID.Equals(matched, StringComparison.OrdinalIgnoreCase))
                {
                    var bookings = await _unitOfWork._bookingRepo.GetBookingsFromCustomer(payment.UserId);
                    var bknd = bookings.FirstOrDefault(x => x.InvoiceId == payment.InvoiceId);
                    if (bknd != null)
                    {
                        bknd.UpdateDate = DateTime.UtcNow;
                        bknd.Status = ConstantEnum.Statuses.COMPLETED;
                        await _unitOfWork._bookingRepo.UpdateAsync(bknd);
                    }
                    payment.UpdateDate = DateTime.UtcNow;
                    payment.PaymentMethod = "Cash On Delivery";
                }

                if (ConstantEnum.Statuses.CANCELLED.Equals(matched, StringComparison.OrdinalIgnoreCase))
                {
                    var bookings = await _unitOfWork._bookingRepo.GetBookingsFromCustomer(payment.UserId);
                    var bknd = bookings.FirstOrDefault(x => x.InvoiceId == payment.InvoiceId);
                    if (bknd != null)
                    {
                        bknd.UpdateDate = DateTime.UtcNow;
                        bknd.Status = ConstantEnum.Statuses.CANCELLED;
                        await _unitOfWork._bookingRepo.UpdateAsync(bknd);
                    }
                    payment.UpdateDate = DateTime.UtcNow;
                }
                await _unitOfWork._paymentRepo.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var paymentView = _mapper.Map<PaymentHistoryView>(payment);
                return paymentView;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
    }
}
