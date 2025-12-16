using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface INotifyRepository : IGenericRepository<PersistNotif>
    {
        Task<List<PersistNotif>?> GetNotifiesByUserIdAsync(Guid userId);
        Task<List<PersistNotif>?> GetAllNotifiesAsync();
        Task<PersistNotif?> GetNotifyByIdAsync(Guid notifyId);
        Task<PersistNotif?> CreateNotify(PersistNotif input);
        Task<PersistNotif?> UpdateNotify(PersistNotif input);
        Task<int> DeleteNotify(Guid id);
        Task<int> DeleteNotifiesByUserId(Guid userId);
    }
}
