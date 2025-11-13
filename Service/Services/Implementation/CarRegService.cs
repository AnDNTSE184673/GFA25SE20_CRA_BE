using AutoMapper;
using Microsoft.AspNetCore.Http;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.ResponseDTO.CarRegister;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarRegService : ICarRegService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UploadFile _file;

        int expirationTimeSec = 1800;

        public CarRegService(UnitOfWork unitOfWork, IMapper mapper, UploadFile file)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _file = file;
        }

        public async Task<(string signedUrl, CarRegView view)> GetCarRegDocById(GetCarRegForm form)
        {
            var url = await _file.CreateSignedUrlAsync(form.)
        }

        public async Task<(string signedUrl, CarRegView view)> GetCarRegDocByPath(GetCarRegForm form)
        {
            var url = await _file.CreateSignedUrlAsync(form.)
        }

        public async Task<(string status, CarRegView regDoc)> SubmitRegisterDocument(IFormFile file, CarRegForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                string uploadDate = DateTime.Now.ToString("ddMMyyyy");

                string originalExt = Path.GetExtension(file.FileName);
                var fileName = $"{form.CarId}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                var imagePath = $"{form.UserId.ToString()}/{fileName}"; //userid/carid_date.ext

                var url = await _file.UploadImageAsync(file, fileName, imagePath, "CarRegistrationDocs", expirationTimeSec);

                //add upload checking logic here

                var mapped = _mapper.Map<CarRegistration>(form);

                mapped.FilePath = imagePath;
                mapped.CreateDate = DateTime.Now;
                mapped.UrlExpiration = mapped.CreateDate.AddSeconds(expirationTimeSec);
                mapped.Status = ConstantEnum.Statuses.PENDING;

                var result = await _unitOfWork._carRegRepo.UploadCarRegistration(mapped);

                await _unitOfWork.CommitTransactionAsync();

                var carReg = _mapper.Map<CarRegView>(result.regData);

                return (result.status, carReg);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
