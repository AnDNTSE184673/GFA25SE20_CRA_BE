using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using PayOS.Exceptions;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.DriverLicense;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.DriverLicense;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.DTO.ResponseDTO.User;
using Repository.Extension.SupabaseFileUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class DriverLicenseService : IDriverLicenseService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly UploadFile _upload;

        int expirationTimeinSeconds = 1800;
        bool isPublic = false;

        public DriverLicenseService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
        }

        public async Task<(string status, ApproveLicenseView view)> ApproveLicenseAsync(LicenseSearchForm form, bool isApproved)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var regs = new List<DriverLicense>();
                var user = new User();

                if (!form.IsValid()) throw new InvalidDataException("Supply either user email or userId!");

                if (form.UserId.HasValue) user = await _unitOfWork._userRepo.GetByIdAsync(form.UserId.Value);
                else if (!form.Email.IsNullOrEmpty()) user = _unitOfWork._userRepo.GetByEmail(form.Email.Trim());
                else throw new Exception("Achievement get: How did we get here?");

                if (user == null) throw new KeyNotFoundException("No user with that email/Id found!");

                regs = await _unitOfWork._driverLicenseRepo.GetLicenseByUserIdAsync(user.Id);

                var mapped = new ApproveLicenseView();

                foreach (var r in regs)
                {
                    if (isApproved)
                    {
                        r.Status = ConstantEnum.Statuses.APPROVED;
                        user.IsVerified = true;
                    }
                    else
                    {
                        r.Status = ConstantEnum.Statuses.DENIED;
                        user.IsVerified = false;
                    }

                    var result = await _unitOfWork._driverLicenseRepo.UpdateLicenseAsync(r);
                    if (result == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (ConstantEnum.RepoStatus.FAILURE, null);
                    }

                    mapped.Document.Add(_mapper.Map<SingleLicenseData>(r));
                }
                var result2 = await _unitOfWork._userRepo.UpdateAsync(user);

                mapped.Owner = _mapper.Map<UserView>(user);

                if (result2 != null)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return (ConstantEnum.RepoStatus.SUCCESS, mapped);
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

        public async Task<(string[] signedUrl, List<DriverLicenseView> view)> GetAllDocumentsAsync()
        {
            var result = await _unitOfWork._driverLicenseRepo.GetAllAsync();
            var uploadTasks = new List<Task<string>>();

            await _upload.EnsureInitializedAsync();

            foreach (var r in result)
            {
                uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeinSeconds));
            }
            var uploadResults = await Task.WhenAll(uploadTasks);
            return (uploadResults, _mapper.Map<List<DriverLicenseView>>(result));
        }

        public async Task<(string[] signedUrl, List<DriverLicenseView> view)> GetDriverLicenseByUser(LicenseSearchForm form)
        {
            List<DriverLicense> result = new List<DriverLicense>();
            if (!form.IsValid()) throw new InvalidDataException("Fill the given parameters!");
            if (form.UserId.HasValue) result = await _unitOfWork._driverLicenseRepo.GetLicenseByUserIdAsync(form.UserId.Value);
            else
            {
                var user = _unitOfWork._userRepo.GetByEmail(form.Email);
                if (user == null) throw new KeyNotFoundException("User with the email not found!");
                result = await _unitOfWork._driverLicenseRepo.GetLicenseByUserIdAsync(user.Id);
            }
            var uploadTasks = new List<Task<string>>();
            await _upload.EnsureInitializedAsync();
            foreach (var r in result)
            {
                uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeinSeconds));
            }
            var uploadResults = await Task.WhenAll(uploadTasks);
            return (uploadResults, _mapper.Map<List<DriverLicenseView>>(result));
        }

        public async Task<DriverLicenseView> UpdateDriverLicenseAsync(List<IFormFile> images, Guid userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var userExist = await _unitOfWork._userRepo.GetByIdAsync(userId);

                if (userExist == null)
                {
                    throw new InvalidOperationException("User doesn't exist!");
                }

                var uploadTasks = new List<Task<(string url, DriverLicense obj)>>();
                int count = 1;
                await _upload.EnsureInitializedAsync();
                foreach (var file in images)
                {
                    uploadTasks.Add(UploadDriverLicenseAsync(file, userId, count));
                    count++;
                }

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                //But ef core operation is sequential
                foreach (var u in uploadResults)
                {
                    await _unitOfWork._driverLicenseRepo.AddDriverLicenseAsync(u.obj);
                }
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var mapped = new DriverLicenseView
                {
                    Urls = urls,
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Status = ConstantEnum.Statuses.PENDING
                };
                return mapped;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string url, DriverLicense obj)> UploadDriverLicenseAsync(IFormFile file, Guid userId, int count)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.DriverLicense;
                string uploadDate = DateTime.UtcNow.AddHours(7).ToString("ddMMyyyy");
                string noExt = Path.GetFileNameWithoutExtension(file.FileName);

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"image{count}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                string imagePath = $"{userId}/{fileName}"; //userid/carid_date.ext

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeinSeconds, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var license = new DriverLicense
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.ACTIVE,
                    UserId = userId
                };

                return (url, license);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
