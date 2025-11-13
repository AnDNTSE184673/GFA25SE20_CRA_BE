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
    public class CarRepository : GenericRepository<Car>, ICarRepository
    {
        public CarRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public Task<string> DeleteCarAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Car>> GetAllCars()
        {
            return await _dbContext.Cars
                .Include(x => x.Owner)
                .Include(x => x.PreferredLot)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<Car> UpdateCarAsync(Car car)
        {
            throw new NotImplementedException();
        }

        public async Task<(string status, Car car)> RegisterCarAsync(Car car)
        {
            try
            {
                var result = await CreateAsync(car);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, car);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
