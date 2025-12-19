using AutoMapper;
using Repository.Base;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class GPSService : IGPSService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GPSService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<GPSView> AddGPS(GPSReceive receive)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var newGPS = new Repository.Data.Entities.GPS
                {
                    Id = Guid.NewGuid(),
                    Latitude = receive.Latitude,
                    Longitude = receive.Longitude,
                    Speed = receive.Speed,
                    CarId = receive.CarId,
                    UserId = receive.UserId,
                    DeviceId = receive.DeviceId,
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork._gpsRepo.CreateAsync(newGPS);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return _mapper.Map<GPSView>(newGPS);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> DeleteGPSOfCar(Guid carId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
                if (car == null) return 0;
                var result = await _unitOfWork._gpsRepo.DeleteGPSDataByCarIdAsync(carId);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<GPSView>> GetAllAsync()
        {
            var gpsData = await _unitOfWork._gpsRepo.GetAllGPSDataAsync();
            if (gpsData == null || !gpsData.Any())
            {
                return new List<GPSView>();
            }
            return _mapper.Map<List<GPSView>>(gpsData);
        }

        public async Task<List<GPSView>> GetByCarIdAsync(Guid carId)
        {
            var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
            if (car == null) return new List<GPSView>();
            var gpsData = await  _unitOfWork._gpsRepo.GetGPSDataByCarIdAsync(carId);
            if (gpsData == null || !gpsData.Any()) return new List<GPSView>();
            return _mapper.Map<List<GPSView>>(gpsData);
        }
    }
}
