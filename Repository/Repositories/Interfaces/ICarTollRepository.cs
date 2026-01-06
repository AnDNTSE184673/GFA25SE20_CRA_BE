using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarTollRepository : IGenericRepository<CarToll>
    {
        Task<CarToll?> GetCarTollDetailsByBookingId(Guid bookingId);
        Task<CarToll?> GetCarTollByBookingNum(string bookingNum);
        Task<CarToll?> GetCarTollDetailsByBookingAndCar(Guid bookingId, Guid carId);
        Task<List<CarToll>?> GetAllCarTolls();
        Task<List<CarToll>?> GetCarTollsByCarId(Guid carId);
        Task<CarToll?> CreateNew(CarToll carToll);
        Task<CarToll?> UpdateCarToll(CarToll carToll);
        Task<CarToll?> InsertToCarToll(CarTollTransac callTollTransac, Guid bookingId, Guid carId);
        Task<bool> DeleteCarToll(Guid carTollId);
        Task<bool> DeleteCarToll(Guid bookingId, Guid carId);
    }
}
