using AutoMapper;
using Repository.Base;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class TollBoothService : ITollBoothService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TollBoothService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<TollBoothView>> GetAllTollBoothsAsync()
        {
            var tollBooths = await _unitOfWork._tollRepo.GetAllAsync();
            return _mapper.Map<List<TollBoothView>>(tollBooths);
        }
    }
}
