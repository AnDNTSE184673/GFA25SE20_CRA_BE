using Repository.Data.Entities;
using Repository.DTO.ResponseDTO.Audits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarHandoverService
    {
        Task<(string status, CarHandoverAudit obj)> CreateCarHandoverInnerServiceAsync(CarHandoverAudit input);
        Task<List<CarHandoverView>> GetCarHandoversAsync();
        Task<List<CarHandoverView>> GetCarHandoversBySchedulesAsync(Guid scheduleId);
    }
}
