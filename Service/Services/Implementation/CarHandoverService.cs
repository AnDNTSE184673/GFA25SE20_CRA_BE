using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.ResponseDTO.Audits;
using Repository.DTO.ResponseDTO.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarHandoverService : ICarHandoverService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CarHandoverService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(string status, CarHandoverAudit obj)> CreateCarHandoverInnerServiceAsync(CarHandoverAudit input)
        {
            try
            {
                var result1 = await _unitOfWork._carHandoverRepo.CreateCarHandoverAsync(input);

                if (result1.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    return (ConstantEnum.RepoStatus.SUCCESS, result1.obj);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CarHandoverView>> GetCarHandoversAsync()
        {
            var result = await _unitOfWork._carHandoverRepo.GetCarHandoversAsync();
            return _mapper.Map<List<CarHandoverView>>(result);
        }

        public async Task<List<CarHandoverView>> GetCarHandoversBySchedulesAsync(Guid scheduleId)
        {
            var result = await _unitOfWork._carHandoverRepo.GetCarHandoversByScheduleAsync(scheduleId);
            return _mapper.Map<List<CarHandoverView>>(result);
        }
    }
}
