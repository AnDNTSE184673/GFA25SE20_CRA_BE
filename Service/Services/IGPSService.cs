using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IGPSService
    {
        Task<List<GPSView>> GetByUserIdAsync(Guid userId);
        Task<List<GPSView>> GetByDeviceIdAsync(string deviceId);
        Task<List<GPSView>> GetAllAsync();
        Task<GPSView> AddGPS(GPSReceive receive);
        Task<GPSView> UpdateGPS(GPSUpdate request);
        Task<int> DeleteGPSOfUser(Guid userId);
        Task<List<GPSView>> DeleteAndLeftLastTwoByUser(Guid userId);
    }
}
