using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
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
        public InvoiceService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Invoice?> CreateInvoice(InvoiceCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var newInvoice = await _unitOfWork._invoiceRepo.CreateInvoice(request);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveChangesAsync();
                return newInvoice;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Invoice>?> GetInvoices()
        {
            return (List<Invoice>?)await _unitOfWork._invoiceRepo.GetAllAsync();
        }

        public async Task<List<Invoice>?> GetInvoicesByCusId(Guid userId)
        {
            var invoices = await _unitOfWork._invoiceRepo.GetInvoiceByCusId(userId);
            if (invoices != null && invoices.Any())
            {
                return invoices;
            }
            return null;
        }

        public async Task<List<Invoice>?> GetInvoicesByVendorId(Guid vendorId)
        {
            var invoices = await _unitOfWork._invoiceRepo.GetInvoiceByVendorId(vendorId);
            if (invoices != null && invoices.Any())
            {
                return invoices;
            }
            return null;
        }

        public async Task<Invoice?> UpdateInvoice(InvoiceUpdateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var invoice = await _unitOfWork._invoiceRepo.UpdateInvoice(request);
                _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransactionAsync();
                return invoice;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to Update Invoice {request.Id}: {ex.Message}", ex);
            }
        }

        public async Task<Invoice?> UpdateInvoiceToCompleted(Guid id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
                invoice.Status = Status.Completed.ToString();
                _unitOfWork._invoiceRepo.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return await _unitOfWork._invoiceRepo.GetInvoiceById(id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to Change Status of Invoice {id} : {ex.Message}", ex);
            }
        }

        public async Task<Invoice?> UpdateInvoiceToFailed(Guid id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var invoice = await _unitOfWork._invoiceRepo.GetInvoiceById(id);
                invoice.Status = Status.Cancelled.ToString();
                _unitOfWork._invoiceRepo.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return await _unitOfWork._invoiceRepo.GetInvoiceById(id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Failed to Change Status of Invoice {id}: {ex.Message}", ex);
            }
        }
    }
}
