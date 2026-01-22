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
    public class TollBoothRepository : GenericRepository<TollBooth>, ITollRepository
    {
        private readonly CRA_DbContext _dbContext;
        public TollBoothRepository(CRA_DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
