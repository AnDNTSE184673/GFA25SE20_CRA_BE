using AutoMapper;
using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarToll;
using Repository.DTO.ResponseDTO.CarToll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarTollService : ICarTollService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CarTollService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<CarTollView?> CreateNewCarToll(CarTollCreateRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var booking = await  _unitOfWork._bookingRepo.GetByIdAsync(request.BookingId);
                if (booking == null)
                {
                    if (!string.IsNullOrEmpty(request.BookingNum))
                    {
                        booking = await  _unitOfWork._bookingRepo.GetBookingFromBookingNum(request.BookingNum);
                        if (booking == null)
                        {
                            throw new Exception("Booking not found");
                        }
                    }
                    else
                    {
                        throw new Exception("Booking not found");
                    }
                }
                var car = await _unitOfWork._carRepo.GetByIdAsync(request.CarId);
                if (car == null)
                {
                    throw new Exception("Car not found");
                }
                var newCarToll = new CarToll
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    BookingNum = booking.BookingNumber,
                    CarId = car.Id,
                    Total = 0,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                var reToll = await _unitOfWork._carTollRepo.CreateNew(newCarToll);
                var carTollTransac = new CarTollTransac
                {
                    Id = Guid.NewGuid(),
                    Amount = request.Amount,
                    TransacDate = DateTime.UtcNow,
                    Status = "Paid",
                    OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                var reTollTransac = await _unitOfWork._carTollRepo.InsertToCarToll(carTollTransac, booking.Id, car.Id);
                if (reToll == null || reTollTransac == null)
                {
                    throw new Exception("Failed to create Car Toll");
                }
                var carWallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(car.Id);
                if (carWallet == null)
                {
                    throw new Exception("Car wallet not found");
                }
                carWallet.Balance -= request.Amount;
                await _unitOfWork._carWalletRepo.UpdateAsync(carWallet);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return _mapper.Map<CarTollView>(reToll);

            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteCarTollByBookingAndCar(Guid bookingId, Guid carId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }
                var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
                if (car == null)
                {
                    throw new Exception("Car not found");
                }
                var result = await  _unitOfWork._carTollRepo.DeleteCarToll(bookingId, carId);
                if (!result)
                {
                    throw new Exception("Failed to delete Car Toll");
                }
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

        public async Task<bool> DeleteCarTollById(Guid carTollId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var result = await  _unitOfWork._carTollRepo.DeleteCarToll(carTollId);
                if (!result)
                {
                    throw new Exception("Failed to delete Car Toll");
                }
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

        public async Task<List<CarTollView>> GetAllCarTolls()
        {
            return _mapper.Map<List<CarTollView>>( await _unitOfWork._carTollRepo.GetAllCarTolls());
        }

        public async Task<CarTollView?> GetCarTollByBookingNum(string bookingNum)
        {
            var booking = await _unitOfWork._bookingRepo.GetBookingFromBookingNum(bookingNum);
            if (booking == null)
            {
                return null;
            }
            var carToll = await _unitOfWork._carTollRepo.GetCarTollByBookingNum(bookingNum);
            return _mapper.Map<CarTollView>(carToll);
        }

        public async Task<CarTollView?> GetCarTollDetailsByBookingAndCar(Guid bookingId, Guid carId)
        {
            var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return null;
            }
            var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
            if (car == null)
            {
                return null;
            }
            var carToll = await  _unitOfWork._carTollRepo.GetCarTollDetailsByBookingAndCar(bookingId, carId);
            return _mapper.Map<CarTollView>(carToll);
        }

        public async Task<CarTollView?> GetCarTollDetailsByBookingId(Guid bookingId)
        {
            var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return null;
            }
            var carToll = await _unitOfWork._carTollRepo.GetCarTollDetailsByBookingId(bookingId);
            return _mapper.Map<CarTollView>(carToll);
        }

        public async Task<CarTollView?> InsertToCarToll(CarTollTransacRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var booking =  _unitOfWork._bookingRepo.GetById(request.BookingId);
                if (booking == null)
                {
                    if (!string.IsNullOrEmpty(request.BookingNum))
                    {
                        booking = await _unitOfWork._bookingRepo.GetBookingFromBookingNum(request.BookingNum);
                        if (booking == null)
                        {
                            throw new Exception("Booking not found");
                        }
                    }
                    else
                    {
                        throw new Exception("Booking not found");
                    }
                }
                var car = await _unitOfWork._carRepo.GetByIdAsync(request.CarId);
                if (car == null)
                {
                    throw new Exception("Car not found");
                }
                var carTollTransac = new CarTollTransac
                {
                    Id = Guid.NewGuid(),
                    Amount = request.Amount,
                    TransacDate = DateTime.UtcNow,
                    Status = "Paid",
                    OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                var updatedCarToll = await _unitOfWork._carTollRepo.InsertToCarToll(carTollTransac, booking.Id, car.Id);
                if (updatedCarToll == null)
                {
                    throw new Exception("Failed to insert to Car Toll");
                }
                var carWallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(car.Id);
                if (carWallet == null)
                {
                    throw new Exception("Car wallet not found");
                }
                carWallet.Balance -= request.Amount;
                await _unitOfWork._carWalletRepo.UpdateAsync(carWallet);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return _mapper.Map<CarTollView>(updatedCarToll);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<CarTollView?> UpdateCarToll(CarTollUpdateRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var existingCarToll = await  _unitOfWork._carTollRepo.GetCarTollDetailsByBookingAndCar(request.BookingId, request.CarId);
                if (existingCarToll == null) {
                    throw new Exception("Car Toll not found");
                }
                existingCarToll.Total = request.Total;
                existingCarToll.UpdateDate = DateTime.UtcNow;
                var updatedCarToll = await _unitOfWork._carTollRepo.UpdateCarToll(existingCarToll);
                if (updatedCarToll == null)
                {
                    throw new Exception("Failed to update Car Toll");
                }
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return _mapper.Map<CarTollView>(updatedCarToll);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public static string GenerateSignature(string amount, string cancelUrl, string description, string orderCode, string returnUrl, string checksumKey)
        {
            string rawData =
                $"amount={amount}&" +
                $"cancelUrl={cancelUrl}&" +
                $"description={description}&" +
                $"orderCode={orderCode}&" +
                $"returnUrl={returnUrl}";

            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public async Task<List<CarTollView>?> GetCarTollsByCarId(Guid carId)
        {
            var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
            if (car == null)
            {
                return null;
            }
            var carTolls = await _unitOfWork._carTollRepo.GetCarTollsByCarId(carId);
            return _mapper.Map<List<CarTollView>>(carTolls);
        }
    }
}
