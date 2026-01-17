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
    public class CarTollRepository : GenericRepository<CarToll>, ICarTollRepository
    {
        private readonly CRA_DbContext _context;
        public CarTollRepository(CRA_DbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<CarToll?> CreateNew(CarToll carToll)
        {
            var result = await _context.CarTolls.AddAsync(carToll);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<bool> DeleteCarToll(Guid carTollId)
        {
            var result = await _context.CarTolls.FirstOrDefaultAsync(ct => ct.Id == carTollId);
            if (result != null)
            {
                _context.CarTolls.Remove(result);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCarToll(Guid bookingId, Guid carId)
        {
            var result = await _context.CarTolls.FirstOrDefaultAsync(ct => ct.BookingId == bookingId && ct.CarId == carId);
            if (result != null)
            {
                _context.CarTolls.Remove(result);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<CarToll>?> GetAllCarTolls()
        {
            return await _context.CarTolls.Include(x => x.CarTollTransacs).ToListAsync();
        }

        public async Task<CarToll?> GetCarTollByBookingNum(string bookingNum)
        {
            return await _context.CarTolls
                .Where(ct => ct.BookingNum == bookingNum)
                .Include(ct => ct.CarTollTransacs)
                .FirstOrDefaultAsync();
        }

        public async Task<CarToll?> GetCarTollDetailsByBookingAndCar(Guid bookingId, Guid carId)
        {
            return await _context.CarTolls
                .Include(ct => ct.CarTollTransacs)
                .Where(ct => ct.BookingId == bookingId && ct.CarId == carId)
                .FirstOrDefaultAsync();
        }

        public async Task<CarToll?> GetCarTollDetailsByBookingId(Guid bookingId)
        {
            return await _context.CarTolls.Include(ct => ct.CarTollTransacs)
                .Where(ct => ct.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CarToll>?> GetCarTollsByCarId(Guid carId)
        {
            return await _context.CarTolls.Include(ct => ct.CarTollTransacs)
                .Where(ct => ct.CarId == carId)
                .ToListAsync();
        }

        public async Task<CarToll?> InsertToCarToll(CarTollTransac callTollTransac, Guid bookingId, Guid carId)
        {
            var carToll = await _context.CarTolls.Include(ct => ct.CarTollTransacs)
                .Where(ct => ct.BookingId == bookingId && ct.CarId == carId)
                .FirstOrDefaultAsync();
            if (carToll != null)
            {
                await _context.CarTollTransacs.AddAsync(callTollTransac);
                carToll.Total += callTollTransac.Amount;
                carToll.UpdateDate = DateTime.UtcNow;
                carToll.CarTollTransacs.Add(callTollTransac);
                await _context.SaveChangesAsync();
                return carToll;
            }
            else
            {
                return null;
            }
        }

        public async Task<CarToll?> UpdateCarToll(CarToll carToll)
        {
            var existingCarToll = await _context.CarTolls.FirstOrDefaultAsync(ct => ct.Id == carToll.Id);
            if (existingCarToll == null)
            {
                return null;
            }
            existingCarToll.Total = carToll.Total;
            existingCarToll.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingCarToll;
        }
    }
}
