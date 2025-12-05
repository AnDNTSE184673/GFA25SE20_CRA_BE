using Microsoft.EntityFrameworkCore;
using Repository.Base;
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
    public class LookupRepository : ILookupRepository
    {
        private readonly CRA_DbContext _context;

        public LookupRepository(CRA_DbContext context)
        {
            _context = context;
        }

        public async Task<List<CarDetailsManufacturer>> GetCarDetailsManufacturer()
        {
            return await _context.CarDetailsManufacturers.AsNoTracking().ToListAsync();
        }

        public async Task<List<CarDetailsModel>> GetCarDetailsModelByManufacturer(int manufacturerId)
        {
            return await _context.CarDetailsModels.AsNoTracking()
                .Where(x => x.ManufacturerId == manufacturerId)
                .ToListAsync();
        }
    }
}
