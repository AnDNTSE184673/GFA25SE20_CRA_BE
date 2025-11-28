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
    public class StaffLogService : IStaffLogService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StaffLogService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(string status, StaffLogAudit obj)> CreateStaffLogInnerServiceAsync(StaffLogAudit input)
        {
            try
            {
                var result1 = await _unitOfWork._staffLogRepo.CreateStaffLogAsync(input);

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

        public async Task<List<StaffLogView>> GetStaffLogsAsync()
        {
            var result = await _unitOfWork._staffLogRepo.GetStaffLogsAsync();
            return _mapper.Map<List<StaffLogView>>(result);
        }

        public async Task<List<StaffLogView>> GetStaffLogsByStaffAsync(Guid staffId)
        {
            var result = await _unitOfWork._staffLogRepo.GetStaffLogsByStaffAsync(staffId);
            return _mapper.Map<List<StaffLogView>>(result);
        }
    }
}
