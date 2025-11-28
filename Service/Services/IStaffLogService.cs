using Repository.Data.Entities;
using Repository.DTO.ResponseDTO.Audits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IStaffLogService
    {
        Task<(string status, StaffLogAudit obj)> CreateStaffLogInnerServiceAsync(StaffLogAudit input);
        Task<List<StaffLogView>> GetStaffLogsAsync();
        Task<List<StaffLogView>> GetStaffLogsByStaffAsync(Guid staffId);
    }
}
