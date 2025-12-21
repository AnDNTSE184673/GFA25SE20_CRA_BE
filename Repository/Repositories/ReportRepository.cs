using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, Report? report)> CreateReport(Report report)
        {
            try
            {
                var result = await CreateAsync(report);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, report);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> RemoveReport(Guid id)
        {
            try
            {
                var existing = await GetByIdAsync(id);
                if (existing == null)
                    return "Report not found";

                await RemoveAsync(existing);

                if (await GetByIdAsync(id) == null)
                    return ConstantEnum.RepoStatus.SUCCESS;
                else
                    return ConstantEnum.RepoStatus.FAILURE;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Report> UpdateReport(Report report)
        {
            try
            {
                var result = await UpdateAsync(report);

                _dbContext.ChangeTracker.Clear();

                return await GetByIdAsync(report.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Report>> GetReportsByCar(Guid carId)
        {
            return await _dbContext.Reports
                .Where(x => x.ReportedCarId.Equals(carId))
                .Include(x => x.Reporter)
                .Include(x => x.Car)
                .ToListAsync();
        }

        public async Task<List<Report>> GetReportsByReportedUser(Guid reportUserId)
        {
            return await _dbContext.Reports
                .Where(x => x.ReportedUserId.Equals(reportUserId))
                .Include(x => x.Reporter)
                .Include(x => x.Reported)
                .ToListAsync();
        }

        public async Task<List<Report>> GetReportsByUser(Guid userId)
        {
            return await _dbContext.Reports
                .Where(x => x.ReporterId.Equals(userId))
                .Include(x => x.Reporter)
                .Include(x => x.Car)
                .Include(x => x.Reported)
                .ToListAsync();
        }

        public async Task<Report> GetReportByReportNo(string reportNo)
        {
            return await _dbContext.Reports
                .Where(x => x.ReportNo.Equals(reportNo.Trim()))
                .Include(x => x.Reporter)
                .Include(x => x.Car)
                .Include(x => x.Reported)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Report>> GetAllReports()
        {
            return await _dbContext.Reports
                .Include(x => x.Reporter)
                .Include(x => x.Car)
                .ToListAsync();
        }
    }
}
