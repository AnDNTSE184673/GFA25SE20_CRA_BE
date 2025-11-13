using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarService : ICarService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;

        public CarService(IMapper mapper, UnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<(string status, CarView car)> RegisterCarAsync(CarInfoForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (!form.UserId.HasValue && string.IsNullOrEmpty(form.Username))
                {
                    return ("Owner name or id is required", null);
                }
                else if (!form.UserId.HasValue)
                {
                    var owner = await _unitOfWork._userRepo.GetUserByUsernameAsync(form.Username);
                    form.UserId = owner.Id;
                }
                if (!form.PrefLotId.HasValue && string.IsNullOrEmpty(form.PrefLotName))
                {
                    return ("Owner name or id is required", null);
                }
                else if (!form.PrefLotId.HasValue)
                {
                    var lot = await _unitOfWork._lotRepo.GetLotByNameAsync(form.PrefLotName);
                    form.PrefLotId = lot.Id;
                }

                var newCar = _mapper.Map<Car>(form);

                newCar.Status = ConstantEnum.Statuses.PENDING;
                newCar.Id = Guid.NewGuid();

                var result = await _unitOfWork._carRepo.RegisterCarAsync(newCar);

                await _unitOfWork.CommitTransactionAsync();

                var info = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(result.car.Id, "Id", x => x.Owner, x => x.PreferredLot);

                var carView = _mapper.Map<CarView>(info);

                return (result.status, carView);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
