using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarTravelLogService
    {
        Task<List<CarTravelView>?> GetAll();
        Task<List<CarTravelView>?> GetByCarId(Guid carId);
        Task<List<CarTravelView>?> GetByBookingId(Guid bookingId);
        Task<List<CarTravelView>?> GetByCarAndBooking(Guid carId, Guid bookingId);
        Task<List<CarTravelView>?> CreateCarTravelLog(Guid carId, Guid bookingId, int tollBoothId);
        Task<List<CarTravelView>?> CreateRandomLog(Guid carId, Guid bookingId, int numOfToll);
    }
}
