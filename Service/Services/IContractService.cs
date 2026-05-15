using Repository.DTO.RequestDTO.Contract;
using Repository.DTO.ResponseDTO.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IContractService
    {
        Task<ContractViewDTO> UploadDocuments(ContractUploadDTO form);
        Task<ContractViewDTO> ViewContractsByBooking(Guid bookingId);
    }
}
