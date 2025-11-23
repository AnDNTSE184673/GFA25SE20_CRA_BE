using Microsoft.EntityFrameworkCore;
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
    public class CarRentalRateRepository : GenericRepository<CarRentalRate>, ICarRentalRateRepository
    {
        public CarRentalRateRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, CarRentalRate obj)> CreateRateAsync(CarRentalRate carRate)
        {
            try
            {
                var result = await CreateAsync(carRate);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, carRate);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteRateAsync(Guid carId)
        {
            try
            {
                var existing = await GetRateByCarAsync(carId);
                if (existing == null)
                    return "Rate for car not found";

                await RemoveAsync(existing);

                _dbContext.ChangeTracker.Clear();

                if (await GetRateByCarAsync(carId) == null)
                    return ConstantEnum.RepoStatus.SUCCESS;
                else
                    return ConstantEnum.RepoStatus.FAILURE;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<CarRentalRate> GetRateByCarAsync(Guid carId)
        {
            return await _dbContext.CarRentalRates
                .Where(x => x.CarId.Equals(carId))
                .Include(x => x.Car)
                .FirstOrDefaultAsync();
        }

        public async Task<CarRentalRate> UpdateRateAsync(CarRentalRate carRate)
        {
            try
            {
                var result = await UpdateAsync(carRate);

                _dbContext.ChangeTracker.Clear();

                return await GetByIdAsync(carRate.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
