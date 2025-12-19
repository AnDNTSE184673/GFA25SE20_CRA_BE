using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IGPSRepository : IGenericRepository<GPS>
    {
        Task<List<GPS>> GetGPSDataByCarIdAsync(Guid carId);
        Task<List<GPS>> GetAllGPSDataAsync();
        Task<int> DeleteGPSDataByCarIdAsync(Guid carId);
    }
}
