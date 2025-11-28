using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IStaffLogRepository : IGenericRepository<StaffLogAudit>
    {
        Task<(string status, StaffLogAudit obj)> CreateStaffLogAsync(StaffLogAudit input);
        Task<List<StaffLogAudit>> GetStaffLogsAsync();
        Task<List<StaffLogAudit>> GetStaffLogsByStaffAsync(Guid staffId);
    }
}
