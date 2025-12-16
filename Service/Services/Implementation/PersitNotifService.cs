using AutoMapper;
using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.PersitNotification;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class PersitNotifService : IPersitNotifService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PersitNotifService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PersitNotifyReturn?> CreateNotif(NotifiCreateRequest input)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var newNotif = new PersistNotif
                {
                    Id = Guid.NewGuid(),
                    Content = input.Content,
                    IsViewed = false,
                    CreateDate = DateTime.UtcNow,
                    UserId = input.UserId
                };
                var createdNotif = await _unitOfWork._notifyRepository.CreateNotify(newNotif);
                if (createdNotif == null) return null;
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<PersitNotifyReturn>(createdNotif);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<int> DeleteNotif(Guid id)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var notify = await _unitOfWork._notifyRepository.GetNotifyByIdAsync(id);
                if (notify == null)
                {
                    return 0;
                }
                var result = await _unitOfWork._notifyRepository.DeleteNotify(notify.Id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransactionAsync().Wait();
                throw;
            }
        }

        public async Task<int> DeleteNotifsByUserId(Guid userId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var user = await _unitOfWork._userRepo.GetByIdAsync(userId);
                if (user == null) return 0;
                var result = await _unitOfWork._notifyRepository.DeleteNotifiesByUserId(userId);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<PersitNotifyReturn>?> GetAllNotif()
        {
            var notifs = await _unitOfWork._notifyRepository.GetAllNotifiesAsync();
            if (notifs == null || !notifs.Any())
            {
                return null;
            }
            return _mapper.Map<List<PersitNotifyReturn>>(notifs);
        }

        public async Task<PersitNotifyReturn?> GetNotifById(Guid notify)
        {
            var notif = await  _unitOfWork._notifyRepository.GetNotifyByIdAsync(notify);
            if (notif == null)
            {
                return null;
            }
            return _mapper.Map<PersitNotifyReturn>(notif);
        }

        public async Task<List<PersitNotifyReturn>?> GetNotifByUserId(Guid userId)
        {
            var user = await _unitOfWork._userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var notifs = await _unitOfWork._notifyRepository.GetNotifiesByUserIdAsync(userId);
            if (notifs == null || !notifs.Any())
            {
                return null;
            }
            return _mapper.Map<List<PersitNotifyReturn>>(notifs);
        }

        public async Task<PersitNotifyReturn?> UpdateNotif(NotiUpdateRequest input)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var notifToUpdate = await _unitOfWork._notifyRepository.GetNotifyByIdAsync(input.Id);
                if (notifToUpdate == null)
                {
                    return null;
                }
                notifToUpdate.IsViewed = input.IsViewed;
                if (input.Content != null)
                {
                    notifToUpdate.Content = input.Content;
                }
                var updatedNotif = await _unitOfWork._notifyRepository.UpdateNotify(notifToUpdate);
                if (updatedNotif == null)
                {
                    return null;
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<PersitNotifyReturn>(updatedNotif);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
