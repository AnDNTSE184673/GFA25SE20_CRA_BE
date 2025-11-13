using Microsoft.AspNetCore.Http;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
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
    }
}
