using Repository.DTO.RequestDTO.CarToll;
using Repository.DTO.ResponseDTO.CarToll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public interface ICarTollService
    {
        Task<List<CarTollView>> GetAllCarTolls();
        Task<CarTollView?> GetCarTollDetailsByBookingId(Guid bookingId);
        Task<CarTollView?> GetCarTollByBookingNum(string bookingNum);
        Task<CarTollView?> GetCarTollDetailsByBookingAndCar(Guid bookingId, Guid carId);
        Task<List<CarTollView>?> GetCarTollsByCarId(Guid carId);
        Task<CarTollView?> CreateNewCarToll(CarTollCreateRequest request);
        Task<CarTollView?> UpdateCarToll(CarTollUpdateRequest request);
        Task<CarTollView?> InsertToCarToll(CarTollTransacRequest request); 
        Task<bool> DeleteCarTollById(Guid carTollId);
        Task<bool> DeleteCarTollByBookingAndCar(Guid bookingId, Guid carId);
    }
}
