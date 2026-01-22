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
    public class CarTravelLogRepository : GenericRepository<CarTravelLog>, ICarTravelLogRepository
    {
        private readonly CRA_DbContext _dbContext;
        public CarTravelLogRepository(CRA_DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CarTravelLog>> GetByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.CarTravelLogs
                .Where(log => log.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<List<CarTravelLog>> GetByCarAndBookingAsync(Guid carId, Guid bookingId)
        {
            return await _dbContext.CarTravelLogs
                .Where(log => log.CarId == carId && log.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<List<CarTravelLog>> GetByCarIdAsync(Guid carId)
        {
            return await _dbContext.CarTravelLogs
                .Where(log => log.CarId == carId)
                .ToListAsync();
        }
    }
}
