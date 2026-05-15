using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task AddContractAsync(Contract obj);
        Task<(string status, Contract? obj)> CreateContractAsync(Contract contract);
        Task<List<Contract>> GetContractsByBookingAsync(Guid bookingId);
        Task<List<Contract>> GetContractsByOnePartyAsync(Guid partyAorBId);
    }
}
