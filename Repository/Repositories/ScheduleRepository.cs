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
    public class ScheduleRepository : GenericRepository<Schedules>, IScheduleRepository
    {
        public ScheduleRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, Schedules? Schedules)> CreateScheduleAsync(Schedules schedules)
        {
            try
            {
                var result = await CreateAsync(schedules);

                _dbContext.ChangeTracker.Clear();

                if (result > 0)
                {
                    var fetch = await GetByIdWithIncludeAsync(schedules.Id, "Id", x => x.Car, x => x.User, x => x.Booking);
                    return (ConstantEnum.RepoStatus.SUCCESS, fetch);
                }   
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<string> DeleteScheduleAsync(Guid id)
        {
            try
            {
                var existing = await GetByIdAsync(id);
                if (existing == null)
                    return "Post not found";

                await RemoveAsync(existing);

                if (await GetByIdAsync(id) == null)
                    return ConstantEnum.RepoStatus.SUCCESS;
                else
                    return ConstantEnum.RepoStatus.FAILURE;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Schedules> GetLastScheduleByBookingAndType(Guid bookingId, string type)
        {
            return await _dbContext.Schedules
                .Where(x => x.BookingId.Equals(bookingId) && x.ScheduleType.Equals(type))
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.Booking)
                .AsNoTracking()
                .LastOrDefaultAsync();
        }

        public async Task<List<Schedules>> GetSchedulesByBooking(Guid bookingId)
        {
            return await _dbContext.Schedules
                .Where(x => x.BookingId.Equals(bookingId))
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.Booking)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Schedules>> GetSchedulesByCar(Guid carId)
        {
            return await _dbContext.Schedules
                .Where(x => x.CarId.Equals(carId))
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.Booking)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Schedules>> GetSchedulesByUser(Guid userId)
        {
            return await _dbContext.Schedules
                .Where(x => x.UserId.Equals(userId))
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.Booking)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Schedules> UpdateScheduleAsync(Schedules schedules)
        {
            try
            {
                var result = await UpdateAsync(schedules);

                _dbContext.ChangeTracker.Clear();

                return await GetByIdWithIncludeAsync(schedules.Id, "Id", x => x.Car, x => x.User);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddScheduleImages(ScheduleImage data)
        {
            try
            {
                var result = await _dbContext.ScheduleImages.AddAsync(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
