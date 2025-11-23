using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarImageRepository : IGenericRepository<CarImage>
    {
        Task AddCarImageAsync(CarImage obj);
        Task<(string status, CarImage? carImage)> CreateCarImageAsync(CarImage carImage);
    }
}
