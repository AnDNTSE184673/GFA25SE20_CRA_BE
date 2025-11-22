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
    public class CarRentalRateRepository : GenericRepository<CarRentalRate>, ICarRentalRateRepository
    {
        public CarRentalRateRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }
    }
}
