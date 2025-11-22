using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRentalRate;
using Repository.DTO.ResponseDTO.CarRentalRate;
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

        public async Task<(string status, CarRentalRateView view)> SetRentalRate(CarRentalRateForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(form.CarId, "Id", x => x.Owner);

                if (car == null) throw new KeyNotFoundException("No car found!");

                if (!form.IsValid().valid) throw new InvalidDataException("At least one rate between daily and hourly has to be filled");

                if(form.IsValid().isHourly && (!form.DailyRate.HasValue || form.DailyRate.Value <= 0))
                {
                    form.DailyRate = form.HourlyRate * 24; //default hour * 24 = day
                }
                else if(!form.IsValid().isHourly && (!form.HourlyRate.HasValue && form.HourlyRate.Value <= 0))
                {
                    form.HourlyRate = form.DailyRate / 24; //default hour * 24 = day
                }

                var newRate = _mapper.Map<CarRentalRate>(form);

                var result = await _unitOfWork._carRentalRateRepo.CreateAsync(newRate);
                await _unitOfWork.CommitTransactionAsync();

                //var lotView = _mapper.Map<ParkingLotView>(result.lot);
                throw new NotImplementedException();

                //return (result.status, lotView);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
