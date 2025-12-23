using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Ocsp;
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
                .Include(x => x.Images.Where(i => i.Status.Equals(ConstantEnum.Statuses.ACTIVE)))
                .AsNoTracking()
                .ToListAsync();
        }
        
        public async Task<List<Car>> GetAllActiveCars()
        {
            return await _dbContext.Cars
                .Include(x => x.Owner)
                .Include(x => x.PreferredLot)
                .Include(x => x.Images.Where(i => i.Status.Equals(ConstantEnum.Statuses.ACTIVE)))
                .Where(x => x.Status.Equals(ConstantEnum.Statuses.ACTIVE))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Car> GetCarByLicensePlate(string licensePlate)
        {
            return await _dbContext.Cars
                .Include(x => x.Owner)
                .Include(x => x.PreferredLot)
                .Include(x => x.Images.Where(i => i.Status.Equals(ConstantEnum.Statuses.ACTIVE)))
                .Where(x => x.Status.Equals(ConstantEnum.Statuses.ACTIVE) 
                && x.LicensePlate.Equals(licensePlate.Trim()))
                .FirstOrDefaultAsync();
        }

        public async Task<Car> UpdateCarAsync(Car car)
        {
            try
            {
                var result = await UpdateAsync(car);

                _dbContext.ChangeTracker.Clear();

                return await GetByIdAsync(car.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
