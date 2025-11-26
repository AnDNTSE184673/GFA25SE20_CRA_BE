using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IScheduleRepository : IGenericRepository<Schedules>
    {
        Task<Schedules> GetLastScheduleByBookingAndType(Guid bookingId, string type);
        Task<List<Schedules>> GetSchedulesByCar(Guid carId);
        Task<List<Schedules>> GetSchedulesByUser(Guid userId);
        Task<List<Schedules>> GetSchedulesByBooking(Guid bookingId);
        Task<(string status, Schedules? Schedules)> CreateScheduleAsync(Schedules schedules);
        Task<string> DeleteScheduleAsync(Guid id);
        Task<Schedules> UpdateScheduleAsync(Schedules schedules);
        Task AddScheduleImages(ScheduleImage data);
        Task<List<ScheduleImage>> GetScheduleImageByBookingAndState(Guid bookingId, bool isCheckIn);
    }
}
