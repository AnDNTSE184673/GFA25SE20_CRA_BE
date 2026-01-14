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
    public class CarWalletRepository : GenericRepository<CarWallet>, ICarWalletRepository
    {
        private readonly CRA_DbContext _context;
        public CarWalletRepository(CRA_DbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<CarWallet?> GetCarWalletByCarId(Guid carId)
        {
            return await _context.CarWallets
                .Where(cw => cw.CarId == carId)
                .FirstOrDefaultAsync();
        }
    }
}
