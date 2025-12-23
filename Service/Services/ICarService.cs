using Microsoft.AspNetCore.Http;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarService
    {
        //Task<CarView> StatusChange
        Task<(string status, CarView car)> RegisterCarAsync(CarInfoForm form);
        Task<CarView> UpdateCarImageAsync(List<IFormFile> images, Guid carId);
        Task<CarView> ChangeCarStatusAsync(CarStatusChange form);
        Task<List<CarView>> GetAllCarsAsync();
        Task<List<CarView>> GetActiveCarsAsync();
        Task<CarView> GetCarByIdAsync(Guid carId);
        Task<List<CarView>> SearchCarAsync(SearchCarForm searchParam);
        Task<CarView> UpdateCarAsync(Guid carId, UpdateCarForm form);
        Task<List<CarDetailsManufacturer>> GetManufacturerLookup();
        Task<List<CarDetailsModel>> GetModelLookupOfManufacturer(int manufacturerId);
    }
}
