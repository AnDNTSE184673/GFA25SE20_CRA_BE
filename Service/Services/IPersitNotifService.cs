using Repository.DTO.RequestDTO.PersitNotification;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IPersitNotifService
    {
        Task<List<PersitNotifyReturn>?> GetAllNotif();
        Task<List<PersitNotifyReturn>?> GetNotifByUserId(Guid userId);
        Task<PersitNotifyReturn?> GetNotifById(Guid notify);
        Task<PersitNotifyReturn?> CreateNotif(NotifiCreateRequest input);
        Task<PersitNotifyReturn?> UpdateNotif(NotiUpdateRequest input);
        Task<int> DeleteNotif(Guid id);
        Task<int> DeleteNotifsByUserId(Guid userId);
    }
}
