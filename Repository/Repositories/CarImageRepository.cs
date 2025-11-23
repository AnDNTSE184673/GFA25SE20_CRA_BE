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
    public class CarImageRepository : GenericRepository<CarImage>, ICarImageRepository
    {
        public CarImageRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddCarImageAsync(CarImage obj)
        {
            try
            {
                var result = await _dbContext.CarImages.AddAsync(obj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, CarImage? carImage)> CreateCarImageAsync(CarImage carImage)
        {
            try
            {
                var result = await CreateAsync(carImage);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, carImage);
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
