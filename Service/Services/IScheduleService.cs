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
        Task<ScheduleView> StatusChangeAsync(Guid scheduleId, bool isCompleted, bool isOverdue);
        Task<(string status, ScheduleView view)> SetMaintenanceAsync(MaintenanceSchedule form);
        Task<List<ScheduleView>> GetAllSchedulesOfUserAsync(Guid userId);
        Task<List<ScheduleView>> GetAllSchedulesOfCarAsync(Guid carId);
        Task<List<ScheduleView>> GetAllSchedulesOfBookingAsync(Guid bookingId);
        Task<(string status, ScheduleView view)> SetCarSchedulesAsync(CreateScheduleForm form);
        Task<(string status, ScheduleView view)> SetCarSchedulesInnerServiceAsync(CreateScheduleForm form);
        Task<ScheduleView> UpdateCarSchedulesAsync(UpdateScheduleForm form);
        Task<string> RemoveSchedulesAsync(Guid scheduleId);
        Task<(string status, ScheduleView view, CICOImageView image)> CheckInAsync(CICOForm form, string userAgent);
        Task<(string status, ScheduleView view, CICOImageView image)> CheckOutAsync(CICOForm form, string userAgent);
        Task<(string status, CICOImageView regDoc)> UploadImageWhenCheckInOut(CheckInOutImages form);
        Task<(string[] signedUrl, CICOImageView view)> GetCICOImageByBooking(CICOImageSearch form);
    }
}
