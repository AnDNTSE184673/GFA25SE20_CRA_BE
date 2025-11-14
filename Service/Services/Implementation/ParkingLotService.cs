using AutoMapper;
using Microsoft.Extensions.Hosting;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.ParkingLot;
using Repository.DTO.ResponseDTO.ParkingLot;
using Repository.Extension.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ParkingLotService(UnitOfWork uoW, IMapper mapper)
        {
            _unitOfWork = uoW;
            _mapper = mapper;
        }

        public async Task<(string status, ParkingLotView lot)> RegisterLotAsync(PostParkingLotForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (!form.ManagerId.HasValue && string.IsNullOrEmpty(form.ManagerName))
                {
                    return ("Manager name or id is required", null);
                }
                else if (!form.ManagerId.HasValue)
                {
                    var manager = await _unitOfWork._userRepo.GetUserByUsernameAsync(form.ManagerName);
                    form.ManagerId = manager.Id;
                }

                var newLot = _mapper.Map<ParkingLot>(form);

                newLot.Id = Guid.NewGuid();
                newLot.Status = ConstantEnum.Statuses.ACTIVE;

                var result = await _unitOfWork._lotRepo.CreateLotAsync(newLot);
                await _unitOfWork.CommitTransactionAsync();

                var lotView = _mapper.Map<ParkingLotView>(result.lot);

                return (result.status, lotView);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ParkingLotView>> GetAllLotAsync()
        {
            var list = await _unitOfWork._lotRepo.GetAllParkingLots();
            return _mapper.Map<List<ParkingLotView>>(list);
        }
    }
}
