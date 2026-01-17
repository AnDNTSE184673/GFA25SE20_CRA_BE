using AutoMapper;
using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarTravelLogService : ICarTravelLogService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CarTravelLogService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<CarTravelView>?> CreateCarTravelLog(Guid carId, Guid bookingId, int tollBoothId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
                if (car == null)
                {
                    throw new Exception("Car not found");
                }
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }
                var toll = await _unitOfWork._tollRepo.GetByIdAsync(tollBoothId);
                if (toll == null)
                {
                    throw new Exception("Toll Booth not found");
                }
                var existingLog = await _unitOfWork._carTravelLogRepo.GetByCarAndBookingAsync(carId, bookingId);
                var wallet = await _unitOfWork._carWalletRepo.GetByIdAsync(carId);
                if (wallet == null)
                {
                    throw new Exception("Car Wallet not found");
                }
                var carToll = await _unitOfWork._carTollRepo.GetCarTollDetailsByBookingId(bookingId);
                if (carToll == null)
                {
                    throw new Exception("Car Toll details not found for the booking");
                }
                var newToll = new CarTollTransac
                {
                    Id = Guid.NewGuid(),
                    OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TransacDate = DateTime.UtcNow,
                    Amount = toll.ChargeAmount,
                    Status = "Completed"
                };
                wallet.Balance -= toll.ChargeAmount;
                await _unitOfWork._carWalletRepo.UpdateAsync(wallet);
                await _unitOfWork._carTollRepo.InsertToCarToll(newToll, bookingId, carId);
                if (existingLog != null )
                {
                    var carTravelLog = new Repository.Data.Entities.CarTravelLog
                    {
                        Id = existingLog.Count > 0 ? existingLog.Max(x => x.Id) + 1 : 1,
                        CarId = carId,
                        BookingId = bookingId,
                        TravelDate = DateTime.UtcNow,
                        TollBoothId = tollBoothId,
                        ChargeAmount = toll.ChargeAmount
                    };
                    await _unitOfWork._carTravelLogRepo.CreateAsync(carTravelLog);
                    await _unitOfWork.CommitTransactionAsync();
                    var result = await _unitOfWork._carTravelLogRepo.GetByCarAndBookingAsync(carId, bookingId);
                    return _mapper.Map<List<CarTravelView>>(result);
                }
                else
                {
                    var carTravelLog = new Repository.Data.Entities.CarTravelLog
                    {
                        Id = 1,
                        CarId = carId,
                        BookingId = bookingId,
                        TravelDate = DateTime.UtcNow,
                        TollBoothId = tollBoothId,
                        ChargeAmount = toll.ChargeAmount
                    };
                    await _unitOfWork._carTravelLogRepo.CreateAsync(carTravelLog);
                    await _unitOfWork.CommitTransactionAsync();
                    var result = await _unitOfWork._carTravelLogRepo.GetByCarAndBookingAsync(carId, bookingId);
                    return _mapper.Map<List<CarTravelView>>(result);
                }
                
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CarTravelView>?> GetAll()
        {
            var carTravelLogs = await _unitOfWork._carTravelLogRepo.GetAllAsync();
            return _mapper.Map<List<CarTravelView>>(carTravelLogs);
        }

        public async Task<List<CarTravelView>?> GetByBookingId(Guid bookingId)
        {
            var result = await _unitOfWork._carTravelLogRepo.GetByBookingIdAsync(bookingId);
            return _mapper.Map<List<CarTravelView>>(result);
        }

        public async Task<List<CarTravelView>?> GetByCarAndBooking(Guid carId, Guid bookingId)
        {
            var result = await _unitOfWork._carTravelLogRepo.GetByCarAndBookingAsync(carId, bookingId);
            return _mapper.Map<List<CarTravelView>>(result);
        }

        public async Task<List<CarTravelView>?> GetByCarId(Guid carId)
        {
            var result = await  _unitOfWork._carTravelLogRepo.GetByCarIdAsync(carId);
            return _mapper.Map<List<CarTravelView>>(result);
        }
    }
}
