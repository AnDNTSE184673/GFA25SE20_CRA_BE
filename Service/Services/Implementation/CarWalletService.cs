using AutoMapper;
using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.ResponseDTO.CarToll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarWalletService : ICarWalletService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CarWalletService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CarWalletView?> AddToCarWallet(Guid carId, decimal amount)
        {
            var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
            if (car == null) throw new Exception();
            var wallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(carId);
            if (wallet == null) throw new Exception();
            wallet.Balance += amount;
            await _unitOfWork._carWalletRepo.UpdateAsync(wallet);
            return _mapper.Map<CarWalletView>(wallet);
        }

        public async Task<CarWalletView?> CreateCarWallet(Guid carId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var car = _unitOfWork._carRepo.GetById(carId);
                if (car == null)
                {
                    throw new Exception("Car not found");
                }
                var carwallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(carId);
                if (carwallet != null) throw new Exception($"Car {carId} already has a wallet");
                var newCarWallet = new CarWallet
                {
                    Id = Guid.NewGuid(),
                    CarId = carId,
                    Balance = 0,
                };
                await _unitOfWork._carWalletRepo.CreateAsync(newCarWallet);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return _mapper.Map<CarWalletView>(newCarWallet);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteCarWallet(Guid carId)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var car = _unitOfWork._carRepo.GetById(carId);
                if (car == null)
                {
                    throw new Exception("Car not found");
                }
                var carWallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(carId);
                if (carWallet == null)
                {
                    throw new Exception("Car wallet not found");
                }
                var result = await _unitOfWork._carWalletRepo.RemoveAsync(carWallet);
                if (!result)
                {
                    throw new Exception("Failed to delete car wallet");
                }
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CarWalletView>> GetAllCarWallets()
        {
            var list = _unitOfWork._carWalletRepo.GetAll();
            return _mapper.Map<List<CarWalletView>>(list);
        }

        public async Task<CarWalletView> GetbyId(Guid id)
        {
            var wallet = await _unitOfWork._carWalletRepo.GetByIdAsync(id);
            if (wallet == null) return new CarWalletView();
            return _mapper.Map<CarWalletView>(wallet);
        }

        public async Task<CarWalletView?> GetCarWalletByCarId(Guid carId)
        {
            var car = _unitOfWork._carRepo.GetById(carId);
            if (car == null) return null;
            var wallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(carId);
            if (wallet == null) return null;
            return _mapper.Map<CarWalletView>(wallet);
        }

        public async Task<CarWalletView?> SubtractFromCarWallet(Guid carId, decimal amount)
        {
            var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
            if (car == null) throw new Exception();
            var wallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(carId);
            if (wallet == null) throw new Exception();
            wallet.Balance -= amount;
            await _unitOfWork._carWalletRepo.UpdateAsync(wallet);
            return _mapper.Map<CarWalletView>(wallet);
        }

        public async Task<CarWalletView?> UpdateCarWalletBalance(Guid carId, decimal amount)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var car = await _unitOfWork._carRepo.GetByIdAsync(carId);
                if (car == null)
                {
                    throw new Exception($"There is no car with id: {carId}");
                }
                var wallet = await _unitOfWork._carWalletRepo.GetCarWalletByCarId(carId);
                if (wallet == null) throw new Exception($"There is no wallet for car with id: {carId}");
                wallet.Balance = amount;
                await _unitOfWork._carWalletRepo.UpdateAsync(wallet);
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
                return _mapper.Map<CarWalletView>(wallet);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw ex;
            }
        }
    }
}
