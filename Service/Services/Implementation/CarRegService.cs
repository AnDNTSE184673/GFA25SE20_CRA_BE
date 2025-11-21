using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Ocsp;
using PayOS.Exceptions;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.User;
using Repository.Extension.SupabaseFileUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarRegService : ICarRegService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UploadFile _upload;

        int expirationTimeSec = 1800;
        bool isPublic = false;

        public CarRegService(UnitOfWork unitOfWork, IMapper mapper, UploadFile file)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _upload = file;
        }

        public async Task<(string status, ApproveRegView view)> ApproveDocumentsAsync(DocumentSearchForm form, bool isApproved)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var regs = new List<CarRegistration>();

                var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(form.CarId.Value, "Id", x => x.Owner);

                if (car == null) throw new NotFoundException("Car doesnt exist!");
                if (car.Owner.Id != form.UserId) throw new UnauthorizedException("Car doesn't belong to this user!");

                if (form.IsValid().isId)
                {
                    regs = await _unitOfWork._carRegRepo.FindCarRegById(form.CarId.Value, form.UserId.Value);
                    if (!regs?.Any() ?? true)
                    {
                        throw new InvalidDataException("Car's documents not found");
                    }
                }
                else
                {
                    regs = await _unitOfWork._carRegRepo.FindCarRegByInfo(form.LicensePlate, form.Email);
                    if (!regs?.Any() ?? true)
                    {
                        throw new InvalidDataException("Car's documents not found");
                    }
                }

                var mapped = new ApproveRegView();

                foreach (var r in regs)
                {
                    if (isApproved)
                    {
                        r.Status = ConstantEnum.Statuses.APPROVED;
                        car.Status = ConstantEnum.Statuses.ACTIVE;
                    }
                    else
                    {
                        r.Status = ConstantEnum.Statuses.DENIED;
                        car.Status = ConstantEnum.Statuses.INACTIVE;
                    }

                    var result = await _unitOfWork._carRegRepo.UpdateCarReg(r);
                    if (result == null) {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (ConstantEnum.RepoStatus.FAILURE, null);
                    }

                    mapped.Document.Add(_mapper.Map<SingleRegData>(r));
                }
                var result2 = await _unitOfWork._carRepo.UpdateCarAsync(car);

                mapped.Owner = _mapper.Map<UserView>(car.Owner);
                mapped.Car = _mapper.Map<CarView>(car);

                if (result2 != null)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return (ConstantEnum.RepoStatus.SUCCESS,mapped);
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return (ConstantEnum.RepoStatus.FAILURE, null);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CarRegView>> GetAllDocumentsAsync()
        {
            var result = await _unitOfWork._carRegRepo.GetCarRegsAsync();
            return _mapper.Map<List<CarRegView>>(result);
        }

        public async Task<(string[] signedUrl, List<CarRegView> view)> GetCarRegDocById(GetCarRegForm form)
        {
            var rows = await _unitOfWork._carRegRepo.FindCarRegById(form.CarId.Value, form.UserId.Value);
            var uploadTasks = new List<Task<string>>();
            foreach(var r in rows)
            {
                uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeSec));
            }
            var uploadResults = await Task.WhenAll(uploadTasks);
            return (uploadResults, _mapper.Map<List<CarRegView>>(rows));
        }

        public async Task<(string[] signedUrl, List<CarRegView> view)> GetCarRegDocByInfo(GetCarRegForm form)
        {
            var rows = await _unitOfWork._carRegRepo.FindCarRegByInfo(form.LicensePlate, form.Email);
            var uploadTasks = new List<Task<string>>();
            foreach (var r in rows)
            {
                uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeSec));
            }
            var uploadResults = await Task.WhenAll(uploadTasks);
            return (uploadResults, _mapper.Map<List<CarRegView>>(rows));
        }

        public async Task<(string[] signedUrl, List<CarRegView> view)> GetCarRegDocByPath(GetCarRegForm form)
        {
            var rows = await _unitOfWork._carRegRepo.FindCarRegByPath(form.FilePath, form.Bucket);
            var uploadTasks = new List<Task<string>>();
            foreach (var r in rows)
            {
                uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeSec));
            }
            var uploadResults = await Task.WhenAll(uploadTasks);
            return (uploadResults, _mapper.Map<List<CarRegView>>(rows));
        }

        public async Task<(string status, CarRegView regDoc)> SubmitRegisterDocument(CarRegForm form)
        {
            var bucket = ConstantEnum.SupabaseBucket.CarRegistration;
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var car = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);
                var user = await _unitOfWork._userRepo.GetByIdAsync(form.UserId);

                if (car == null || user == null) throw new NotFoundException("User or car not found!");

                int count = 1;
                var uploadTasks = new List<Task<(string url, CarRegistration obj)>>();
                foreach(var file in form.images)
                {
                    uploadTasks.Add(UploadRegDocsAsync(file, form.CarId, form.UserId, count));
                    count++;
                }

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                foreach(var u in uploadResults)
                {
                    await _unitOfWork._carRegRepo.AddCarRegistration(u.obj);
                }

                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if(result <= 0)
                {
                    return (ConstantEnum.RepoStatus.FAILURE, null);
                }
                else
                {
                    /*var objList = uploadResults.Select(r => {
                            return r.obj;
                        }).ToList();
                    carRegViews = _mapper.Map<List<CarRegView>>(objList);*/
                    var carRegView = new CarRegView
                    {
                        CarId = form.CarId,
                        UserId = form.UserId,
                        Urls = urls,
                        CreateDate = DateTime.UtcNow,
                        Status = ConstantEnum.Statuses.PENDING
                    };
                    return (ConstantEnum.RepoStatus.SUCCESS, carRegView);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string url, CarRegistration obj)> UploadRegDocsAsync(IFormFile file, Guid carId, Guid userId, int count)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.CarRegistration;
                string uploadDate = DateTime.UtcNow.ToString("ddMMyyyy");

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"image{count}_{uploadDate}{originalExt}";
                string imagePath = $"{userId}/{carId}/{fileName}"; 

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeSec, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var regImage = new CarRegistration
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.PENDING,
                    CarId = carId,
                    UserId = userId
                };

                return (url, regImage);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
