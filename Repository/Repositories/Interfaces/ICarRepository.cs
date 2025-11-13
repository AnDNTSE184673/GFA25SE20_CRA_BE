using Microsoft.Extensions.Hosting;
using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarRepository : IGenericRepository<Car>
    {
        Task<(string status, Car car)> RegisterCarAsync(Car car);
        Task<Car> UpdateCarAsync(Car car);
        Task<string> DeleteCarAsync(Guid id);
        Task<List<Car?>> GetAllCars();
    }
}
