using AutoMapper;
using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repository.Constant.ConstantEnum;

namespace Service.Services.Implementation
{
    public class InvoiceService : IInvoiceService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public InvoiceService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<InvoiceView?> CreateInvoice(InvoiceCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var newInvoice = await _unitOfWork._invoiceRepo.CreateInvoice(request);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork._paymentRepo.CreateNewPaymentForBookingFee(newInvoice.Id);
                await _unitOfWork._paymentRepo.CreateNewPaymentForRentalFee(newInvoice.Id);
                await _unitOfWork.SaveChangesAsync();
                var InvoiceResult = await _unitOfWork._invoiceRepo.GetInvoiceById(newInvoice.Id);
                var ListInvoiceView = _mapper.Map<InvoiceView>(InvoiceResult);
                return ListInvoiceView;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<InvoiceView?> GetInvoiceById(Guid id)
        {
            var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
            if (invoice != null)
            {
                return _mapper.Map<InvoiceView>(invoice);
            }
            return null;
        }

        public async Task<List<InvoiceView>?> GetInvoices()
        {
            var allInvoices = await _unitOfWork._invoiceRepo.GetAllInvoices();
            if (allInvoices != null && allInvoices.Any())
            {
                return _mapper.Map<List<InvoiceView>>(allInvoices);
            }
            return null;
        }

        public async Task<List<InvoiceView>?> GetInvoicesByCusId(Guid userId)
        {
            var invoices = await _unitOfWork._invoiceRepo.GetInvoiceByCusId(userId);
            if (invoices != null && invoices.Any())
            {
                return _mapper.Map<List<InvoiceView>>(invoices);
            }
            return null;
        }

        public async Task<List<InvoiceView>?> GetInvoicesByVendorId(Guid vendorId)
        {
            var invoices = await _unitOfWork._invoiceRepo.GetInvoiceByVendorId(vendorId);
            if (invoices != null && invoices.Any())
            {
                return _mapper.Map<List<InvoiceView>>(invoices);
            }
            return null;
        }

        public async Task<InvoiceView?> UpdateInvoice(InvoiceUpdateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var invoice = await _unitOfWork._invoiceRepo.UpdateInvoice(request);
                _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<InvoiceView>(invoice);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to Update Invoice {request.Id}: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceView?> UpdateInvoiceToCompleted(Guid id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
                invoice.Status = Status.Completed.ToString();
                _unitOfWork._invoiceRepo.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                var completedIn = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
                return _mapper.Map<InvoiceView>(completedIn);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to Change Status of Invoice {id} : {ex.Message}", ex);
            }
        }

        public async Task<InvoiceView?> UpdateInvoiceToFailed(Guid id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
                invoice.Status = Status.Cancelled.ToString();
                _unitOfWork._invoiceRepo.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                var failedIn = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
                return _mapper.Map<InvoiceView>(failedIn);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to Change Status of Invoice {id}: {ex.Message}", ex);
            }
        }
    }
}
