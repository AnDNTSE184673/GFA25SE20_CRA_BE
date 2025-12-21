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
        Task<List<GPS>> GetGPSDataByUserIdAsync(Guid userId);
        Task<List<GPS>> GetGPSDataByDeviceIdAsync(string deviceId);
        Task<GPS> GetGPSByUserAndDevice(Guid userId, string deviceId);
        Task<List<GPS>> GetAllGPSDataAsync();
        Task<int> DeleteAllGPSDataByUserIdAsync(Guid userId);
        Task<List<GPS>> DeleteAndLeftLastTwoByUser(Guid userId);
    }
}
