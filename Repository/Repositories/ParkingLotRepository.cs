using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ParkingLotRepository : GenericRepository<ParkingLot>, IParkingLotRepository
    {
        public ParkingLotRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public Task<string> DeleteLotAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ParkingLot?>> GetAllParkingLots()
        {
            return await _dbContext.ParkingLots
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(string status, ParkingLot lot)> CreateLotAsync(ParkingLot lot)
        {
            try
            {
                var result = await CreateAsync(lot);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, lot);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<ParkingLot> UpdateLotAsync(ParkingLot lot)
        {
            throw new NotImplementedException();
        }

        public async Task<ParkingLot> GetLotByNameAsync(string? prefLotName)
        {
            return await _dbContext.ParkingLots
                .Where(x => x.Name.Equals(prefLotName))
                .Include(x => x.Manager)
                .FirstOrDefaultAsync();
        }
    }
}
