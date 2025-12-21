using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IReportRepository : IGenericRepository<Report>
    {
        Task<(string status, Report? report)> CreateReport(Report report);
        Task<string> RemoveReport(Guid id);
        Task<Report> UpdateReport(Report report);
        Task<Report> GetReportByReportNo(string reportNo);
        Task<List<Report>> GetReportsByUser(Guid userId);
        Task<List<Report>> GetReportsByReportedUser(Guid reportUserId);
        Task<List<Report>> GetReportsByCar(Guid carId);
        Task<List<Report>> GetAllReports();
    }
}
