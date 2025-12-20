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
    public class GPSRepository : GenericRepository<GPS>, IGPSRepository
    {
        private readonly CRA_DbContext _context;
        public GPSRepository(CRA_DbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> DeleteGPSDataByCarIdAsync(Guid carId)
        {
            var gpsData = await _context.GPS.Where(gps => gps.CarId == carId).ToListAsync();
            if (!gpsData.Any())
            {
                return 0;
            }
            _context.RemoveRange(gpsData);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<GPS>> GetAllGPSDataAsync()
        {
            return await _context.GPS
                .OrderByDescending(gps => gps.Timestamp)
                .ToListAsync();
        }

        public async Task<List<GPS>> GetGPSDataByCarIdAsync(Guid carId)
        {
            return await _context.GPS
                .Where(gps => gps.CarId == carId)
                .OrderByDescending(gps => gps.Timestamp)
                .ToListAsync();
        }

        public async Task<List<GPS>> GetGPSDataByDeviceIdAsync(string deviceId)
        {
            return await _context.GPS
                .Where(gps => gps.DeviceId == deviceId)
                .OrderByDescending(gps => gps.Timestamp)
                .ToListAsync();
        }

        public async Task<List<GPS>> GetGPSDataByUserIdAsync(Guid userId)
        {
            return await _context.GPS
                .Where(gps => gps.UserId == userId)
                .OrderByDescending(gps => gps.Timestamp)
                .ToListAsync();
        }
    }
}
