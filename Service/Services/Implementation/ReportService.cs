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
using System.Reactive.Subjects;
using Supabase.Gotrue;
using Microsoft.Extensions.Configuration;

namespace Service.Services.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public ReportService(IMapper mapper, UnitOfWork unitOfWork, IConfiguration config)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _config = config;
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

        public async Task<List<ReportView>> GetReportsByReportedUser(Guid userId)
        {
            var result = await _unitOfWork._reportRepo.GetReportsByReportedUser(userId);
            var reportViews = _mapper.Map<List<ReportView>>(result);

            return reportViews;
        }

        public async Task<List<ReportView>> GetAllReports()
        {
            var result = await _unitOfWork._reportRepo.GetAllReports();
            var reportViews = _mapper.Map<List<ReportView>>(result);

            return reportViews;
        }

        public async Task<ReportView> ApproveCarReport(ApproveReportForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                Report reportExist = new Report();

                if (!form.ReportNo.IsNullOrEmpty()) reportExist = await _unitOfWork._reportRepo.GetReportByReportNo(form.ReportNo);
                else if (!form.ReportId.HasValue) reportExist = await _unitOfWork._reportRepo.GetByIdAsync(form.ReportId.Value);
                else throw new InvalidOperationException("Must fill in one field!");

                if (reportExist == null) throw new KeyNotFoundException("No report with the given number or Id found!");
                var car = await _unitOfWork._carRepo.GetByIdAsync(reportExist.ReportedCarId.Value);

                if (form.isApproved)
                {
                    reportExist.Status = ConstantEnum.Statuses.CONFIRMED;
                    car.Status = ConstantEnum.Statuses.INACTIVE;
                    await _unitOfWork._carRepo.UpdateCarAsync(car);
                }
                else
                {
                    reportExist.Status = ConstantEnum.Statuses.DENIED;
                }

                var result = await _unitOfWork._reportRepo.UpdateReport(reportExist);
                var notify = new PersistNotif
                {
                    Id = Guid.NewGuid(),
                    Content = form.isApproved ? $"The report for Car {car.LicensePlate} has been approved. The car is now deactivatied" : $"The report for Car {car.LicensePlate} has been denied.",
                    IsViewed = false,
                    CreateDate = DateTime.UtcNow,
                    UserId = reportExist.ReporterId,
                };
                await _unitOfWork._notifyRepository.CreateNotify(notify);

                if (result == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }
                else
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return _mapper.Map<ReportView>(reportExist);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ReportView view)> CreateCarReport(CarReportForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var carExist = await _unitOfWork._carRepo.GetByIdAsync(form.ReportedCarId);
                var userExist = await _unitOfWork._userRepo.GetByIdAsync(form.ReporterId);

                if (carExist == null || userExist == null)
                    throw new KeyNotFoundException("User or Car not found!");

                var mapped = _mapper.Map<Report>(form);
                mapped.Id = Uuid7.NewGuid();
                mapped.ReportNo = $"C-RP{mapped.Id.ToString().Split('-').Last().Substring(0, 12).ToUpper()}";
                mapped.CreateDate = DateTime.UtcNow;
                mapped.Status = ConstantEnum.Statuses.ACTIVE;

                var carNoti = new PersistNotif
                {
                    Id = Guid.NewGuid(),
                    Content = $"Your car with ID: {carExist.Id} has been reported for '{form.Title}'. Please check the report for more details.",
                    IsViewed = false,
                    CreateDate = DateTime.UtcNow,
                    UserId = carExist.UserId,
                };
                await _unitOfWork._notifyRepository.CreateNotify(carNoti);

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

        public async Task<(string status, ReportView view)> CreateUserReport(UserReportForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var reportedUserExist = await _unitOfWork._userRepo.GetByIdAsync(form.ReportedUserId);
                var reporterExist = await _unitOfWork._userRepo.GetByIdAsync(form.ReporterId);

                if (reportedUserExist == null || reporterExist == null)
                    throw new KeyNotFoundException("Either or both users not found!");

                var mapped = _mapper.Map<Report>(form);
                mapped.Id = Uuid7.NewGuid();
                mapped.ReportNo = $"U-RP{mapped.Id.ToString().Split('-').Last().Substring(0, 12).ToUpper()}";
                mapped.CreateDate = DateTime.UtcNow;
                mapped.Status = ConstantEnum.Statuses.ACTIVE;

                var result = await _unitOfWork._reportRepo.CreateReport(mapped);               

                reportedUserExist.BehaviourScore = reportedUserExist.BehaviourScore - form.deductedPoints;
                //first report is 66, second report is 32, third report is -2 which will hit this condition (old)

                var userNoti = new PersistNotif
                {
                    Id = Guid.NewGuid(),
                    Content = $"You have been reported for '{form.Title}'. Your account health is now {reportedUserExist.BehaviourScore}. Check the report for more details. Please be more carefull next time",
                    IsViewed = false,
                    CreateDate = DateTime.UtcNow,
                    UserId = reportedUserExist.Id,
                };
                await _unitOfWork._notifyRepository.CreateNotify(userNoti);

                if (reportedUserExist.BehaviourScore <= 0) reportedUserExist.Status = ConstantEnum.Statuses.CLOSED;
                await _unitOfWork._userRepo.UpdateAsync(reportedUserExist);

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
