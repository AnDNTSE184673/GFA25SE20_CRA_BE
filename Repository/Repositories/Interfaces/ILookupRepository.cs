using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ILookupRepository
    {
        Task<List<CarDetailsManufacturer>> GetCarDetailsManufacturer();
        Task<List<CarDetailsModel>> GetCarDetailsModelByManufacturer(int manufacturerId);
    }
}
