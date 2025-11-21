using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using Supabase.Gotrue;
using Supabase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class CarRegRepository : GenericRepository<CarRegistration>, ICarRegRepository
    {
        public CarRegRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<CarRegistration>> GetCarRegsAsync()
        {
            return await _dbContext.CarRegistrations
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<CarRegistration>> FindCarRegById(Guid carId, Guid userId)
        {
            return await _dbContext.CarRegistrations
                .Where(x => x.CarId.Equals(carId) && x.UserId.Equals(userId))
                .ToListAsync();
        }

        public async Task<List<CarRegistration>> FindCarRegByPath(string filePath, string bucket)
        {
            return await _dbContext.CarRegistrations
                .Where(x => x.FilePath.Equals(filePath) && x.Bucket.Equals(bucket))
                .ToListAsync();
        }

        public async Task<(string status, CarRegistration regData)> UploadCarRegistration(CarRegistration data)
        {
            try
            {
                var result = await CreateAsync(data);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, data);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddCarRegistration(CarRegistration data)
        {
            try
            {
                var result = await _dbContext.CarRegistrations.AddAsync(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<CarRegistration> UpdateCarReg(CarRegistration reg)
        {
            try
            {
                var result = await UpdateAsync(reg);

                return reg;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CarRegistration>> FindCarRegByInfo(string licensePlate, string email)
        {
            return await _dbContext.CarRegistrations
                .Include(x => x.Car)
                .Include(x => x.Owner)
                .Where(x => x.Car.LicensePlate.Equals(licensePlate) && x.Owner.Email.Equals(email))
                .ToListAsync();
        }
    }
}
