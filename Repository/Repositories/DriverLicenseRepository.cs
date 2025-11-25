using Microsoft.EntityFrameworkCore;
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
using static System.Net.Mime.MediaTypeNames;

namespace Repository.Repositories
{
    public class DriverLicenseRepository : GenericRepository<DriverLicense>, IDriverLicenseRepository
    {
        public DriverLicenseRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddDriverLicenseAsync(DriverLicense license)
        {
            try
            {
                var result = await _dbContext.DriverLicenses.AddAsync(license);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, DriverLicense? license)> CreateDriverLicenseAsync(DriverLicense license)
        {
            try
            {
                var result = await CreateAsync(license);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, license);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<DriverLicense>> GetLicenseByUserIdAsync(Guid id)
        {
            return await _dbContext.DriverLicenses
                .Where(x => x.UserId.Equals(id))
                .Include(x => x.Owner)
                .ToListAsync();
        }

        public async Task<DriverLicense> UpdateLicenseAsync(DriverLicense license)
        {
            try
            {
                var result = await UpdateAsync(license);

                _dbContext.ChangeTracker.Clear();

                return await GetByIdAsync(license.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
