using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class PersitNotifRepository : GenericRepository<PersistNotif>, INotifyRepository
    {
        private readonly CRA_DbContext _context;
        public PersitNotifRepository(CRA_DbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PersistNotif?> CreateNotify(PersistNotif input)
        {
            var entity = await _context.PersistNotifs.AddAsync(input);
            _context.SaveChanges();
            return entity.Entity;
        }

        public async Task<int> DeleteNotifiesByUserId(Guid userId)
        {
            var notifies = _context.PersistNotifs.Where(n => n.UserId == userId);
            _context.PersistNotifs.RemoveRange(notifies);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteNotify(Guid id)
        {
            var notify = await  _context.PersistNotifs.FindAsync(id);
            if (notify != null)
            {
                _context.PersistNotifs.Remove(notify);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<List<PersistNotif>?> GetAllNotifiesAsync()
        {
            return await _context.PersistNotifs.Include(n => n.User).OrderByDescending(n => n.CreateDate).ToListAsync();
        }

        public async Task<List<PersistNotif>?> GetNotifiesByUserIdAsync(Guid userId)
        {
            var notifies = await  _context.PersistNotifs
                .Include(n => n.User)
                .OrderByDescending(n => n.CreateDate)
                .Where(n => n.UserId == userId).ToListAsync();
            return notifies;
        }

        public async Task<PersistNotif?> GetNotifyByIdAsync(Guid notifyId)
        {
            return await _context.PersistNotifs
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == notifyId);
        }

        public async Task<PersistNotif?> UpdateNotify(PersistNotif input)
        {
            var entity =  _context.PersistNotifs.Update(input);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }
    }
}
