using Repository.DTO.RequestDTO.ParkingLot;
using Repository.DTO.ResponseDTO.ParkingLot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IParkingLotService
    {
        Task<List<ParkingLotView>> GetAllLotAsync();
        Task<(string status, ParkingLotView lot)> RegisterLotAsync(PostParkingLotForm form);
    }
}
