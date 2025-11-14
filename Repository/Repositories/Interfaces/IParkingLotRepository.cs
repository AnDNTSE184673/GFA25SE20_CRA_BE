using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IParkingLotRepository : IGenericRepository<ParkingLot>
    {
        Task<(string status, ParkingLot lot)> CreateLotAsync(ParkingLot lot);
        Task<ParkingLot> UpdateLotAsync(ParkingLot lot);
        Task<string> DeleteLotAsync(Guid id);
        Task<List<ParkingLot?>> GetAllParkingLots();
        Task<ParkingLot> GetLotByNameAsync(string? prefLotName);
    }
}
