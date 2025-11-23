using Repository.DTO.RequestDTO.Schedule;
using Repository.DTO.ResponseDTO.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleView>> GetAllSchedulesOfUserAsync(Guid userId);
        Task<List<ScheduleView>> GetAllSchedulesOfCarAsync(Guid carId);
        Task<(string status, ScheduleView view)> SetCarSchedulesAsync(CreateScheduleForm form);
        Task<ScheduleView> UpdateCarSchedulesAsync(UpdateScheduleForm form);
        Task<string> RemoveCarSchedulesAsync(Guid carId);
    }
}
