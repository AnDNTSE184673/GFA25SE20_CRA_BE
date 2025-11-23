using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRentalRate;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRentalRate;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.DTO.ResponseDTO.ParkingLot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarRentalService : ICarRentalService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;

        public CarRentalService(IMapper mapper, UnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> DeleteRentalRate(Guid carId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var result = await _unitOfWork._carRentalRateRepo.DeleteRateAsync(carId);

                await _unitOfWork.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<CarRentalRateView> GetCarRentalRate(Guid carId)
        {
            var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(carId, "Id", x => x.Owner);
            if (car == null) throw new KeyNotFoundException("No car found!");
            var rate = await _unitOfWork._carRentalRateRepo.GetRateByCarAsync(carId);
            return _mapper.Map<CarRentalRateView>(rate);
        }

        public async Task<(string status, CarRentalRateView view)> SetRentalRate(CreateCarRentalRateForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(form.CarId, "Id", x => x.Owner);

                if (car == null) throw new KeyNotFoundException("No car found!");

                var exist = await _unitOfWork._carRentalRateRepo.GetRateByCarAsync(form.CarId);

                if (exist != null) throw new Exception("Rate for this car already exist! Delete or update only!");

                if (!form.IsValid().valid) throw new InvalidDataException("At least one rate between daily and hourly has to be filled");

                if (form.IsValid().isHourly)
                {
                    if (!(form.DailyRate <= 0.0))
                    {
                        form.DailyRate = form.HourlyRate * 24;
                    }
                }
                else
                {
                    if (!(form.HourlyRate <= 0.0))
                    {
                        form.HourlyRate = form.DailyRate / 24;
                    }
                }

                var newRate = _mapper.Map<CarRentalRate>(form);

                newRate.Status = ConstantEnum.Statuses.ACTIVE;

                var result = await _unitOfWork._carRentalRateRepo.CreateRateAsync(newRate);
                await _unitOfWork.CommitTransactionAsync();

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var mapped = _mapper.Map<CarRentalRateView>(newRate);
                    mapped.Car = _mapper.Map<CarView>(car);
                    return (result.status, mapped);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<CarRentalRateView> UpdateRentalRate(UpdateCarRentalRateForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _unitOfWork._carRentalRateRepo.GetRateByCarAsync(form.CarId);
                if (existing == null)
                    throw new KeyNotFoundException("Car's rental rate not found!");

                var mapped = _mapper.Map(form, existing);

                var result = await _unitOfWork._carRentalRateRepo.UpdateRateAsync(mapped);

                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<CarRentalRateView>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
