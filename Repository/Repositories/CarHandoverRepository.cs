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
    public class CarHandoverRepository : GenericRepository<CarHandoverAudit>, ICarHandoverRepository
    {
        public CarHandoverRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, CarHandoverAudit obj)> CreateCarHandoverAsync(CarHandoverAudit input)
        {
            try
            {
                var result = await CreateAsync(input);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, input);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CarHandoverAudit>> GetCarHandoversAsync()
        {
            return await _dbContext.CarHandoverAudits
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<CarHandoverAudit>> GetCarHandoversByScheduleAsync(Guid scheduleId)
        {
            return await _dbContext.CarHandoverAudits
                .AsNoTracking()
                .Where(x => x.ScheduleId.Equals(scheduleId))
                .ToListAsync();
        }
    }
}
