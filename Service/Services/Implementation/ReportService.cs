using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.RequestDTO.Report;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.DTO.ResponseDTO.Report;
using Repository.Extension.SupabaseFileUploader;
using Supabase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medo;

namespace Service.Services.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;

        public ReportService(IMapper mapper, UnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        
        public async Task<string> DeleteCarReport(Guid id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var result = await _unitOfWork._reportRepo.RemoveReport(id);

                await _unitOfWork.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReportView> EditCarReport(Guid id, EditReportForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = _unitOfWork._reportRepo.GetById(id);
                if (existing == null)
                    return null;

                //partial mapping
                var report = _mapper.Map(form, existing);
                var result = await _unitOfWork._reportRepo.UpdateReport(report);

                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<ReportView>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ReportView>> GetCarReports(Guid carId)
        {
            var result = await _unitOfWork._reportRepo.GetReportsByCar(carId);
            var reportViews = _mapper.Map<List<ReportView>>(result);

            return reportViews;
        }
        
        public async Task<List<ReportView>> GetReportsByUser(Guid userId)
        {
            var result = await _unitOfWork._reportRepo.GetReportsByUser(userId);
            var reportViews = _mapper.Map<List<ReportView>>(result);

            return reportViews;
        }

        public async Task<List<ReportView>> GetAllReports()
        {
            var result = await _unitOfWork._reportRepo.GetAllReports();
            var reportViews = _mapper.Map<List<ReportView>>(result);

            return reportViews;
        }

        public async Task<(string status, ReportView view)> CreateCarReport(ReportForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var carExist = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);
                var userExist = await _unitOfWork._userRepo.GetByIdAsync(form.UserId);

                if (carExist == null || userExist == null)
                    throw new KeyNotFoundException("User or Car not found!");

                var mapped = _mapper.Map<Report>(form);
                mapped.Id = Uuid7.NewGuid();
                mapped.ReportNo = $"RP{mapped.Id.ToString().Split('-').Last().Substring(0, 12).ToUpper()}";
                mapped.CreateDate = DateTime.UtcNow;
                mapped.Status = ConstantEnum.Statuses.ACTIVE;

                var result = await _unitOfWork._reportRepo.CreateReport(mapped);
                await _unitOfWork.CommitTransactionAsync();

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var view = _mapper.Map<ReportView>(result.report);
                    return (result.status, view);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        
    }
}
