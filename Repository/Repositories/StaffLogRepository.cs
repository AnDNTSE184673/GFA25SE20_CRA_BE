using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class StaffLogRepository : GenericRepository<StaffLogAudit>, IStaffLogRepository
    {
        public StaffLogRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, StaffLogAudit obj)> CreateStaffLogAsync(StaffLogAudit input)
        {
            try
            {
                var result = await CreateAsync(input);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, input);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<StaffLogAudit>> GetStaffLogsAsync()
        {
            return await _dbContext.StaffLogAudit
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<StaffLogAudit>> GetStaffLogsByStaffAsync(Guid staffId)
        {
            return await _dbContext.StaffLogAudit
                .AsNoTracking()
                .Where(x => x.StaffId.Equals(staffId))
                .ToListAsync();
        }
    }
}
