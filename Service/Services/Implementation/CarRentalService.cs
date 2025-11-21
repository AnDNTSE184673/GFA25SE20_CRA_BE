using AutoMapper;
using Repository.Base;
using Repository.DTO.RequestDTO.CarRentalRate;
using Repository.DTO.ResponseDTO;
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

        public Task<(string status, CarRentalRateView view)> SetRentalRate(CarRentalRateForm form)
        {
            throw new NotImplementedException();
        }
    }
}
