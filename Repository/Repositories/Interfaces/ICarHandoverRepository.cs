using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarHandoverRepository : IGenericRepository<CarHandoverAudit>
    {
        Task<(string status, CarHandoverAudit obj)> CreateCarHandoverAsync(CarHandoverAudit input);
        Task<List<CarHandoverAudit>> GetCarHandoversAsync();
        Task<CarHandoverAudit> GetCarHandoverByScheduleAsync(Guid scheduleId);
    }
}
