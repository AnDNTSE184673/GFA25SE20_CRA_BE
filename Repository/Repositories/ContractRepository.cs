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
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddContractAsync(Contract obj)
        {
            try
            {
                var result = await _dbContext.Contracts.AddAsync(obj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, Contract? obj)> CreateContractAsync(Contract contract)
        {
            try
            {
                var result = await CreateAsync(contract);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, contract);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Contract>> GetContractsByBookingAsync(Guid bookingId)
        {
            return await _dbContext.Contracts
                .Where(c => c.BookingId.Equals(bookingId))
                .ToListAsync();
        }

        public async Task<List<Contract>> GetContractsByOnePartyAsync(Guid partyAorBId)
        {
            return await _dbContext.Contracts
                .Where(c => c.PartyAId.Equals(partyAorBId) || c.PartyBId.Equals(partyAorBId))
                .ToListAsync();
        }
    }
}
