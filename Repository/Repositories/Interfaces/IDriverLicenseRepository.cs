using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IDriverLicenseRepository : IGenericRepository<DriverLicense>
    {
        Task<(string status, DriverLicense? license)> CreateDriverLicenseAsync(DriverLicense license);
        Task AddDriverLicenseAsync(DriverLicense license);
        Task<List<DriverLicense>> GetLicenseByUserAsync(Guid id);
        Task<DriverLicense> UpdateLicenseAsync(DriverLicense license);
    }
}
