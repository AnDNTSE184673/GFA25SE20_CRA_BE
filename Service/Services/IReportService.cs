using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.RequestDTO.Report;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.DTO.ResponseDTO.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IReportService
    {
        Task<List<ReportView>> GetCarReports(Guid carId);
        Task<List<ReportView>> GetReportsByUser(Guid userId);
        Task<List<ReportView>> GetReportsByReportedUser(Guid reportUserId);
        Task<List<ReportView>> GetAllReports();
        Task<ReportView> ApproveCarReport(ApproveReportForm form);
        Task<(string status, ReportView view)> CreateCarReport(CarReportForm form);
        Task<(string status, ReportView view)> CreateUserReport(UserReportForm form);
        Task<ReportView> EditCarReport(Guid id, EditReportForm form);
        Task<string> DeleteCarReport(Guid id);
    }
}
