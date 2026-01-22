using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
using System.Globalization;
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
        private readonly IFPTAIService _fptAI;

        private int expirationTimeinSeconds = 1800;
        private bool isPublic = false;

        public DriverLicenseService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload, IFPTAIService fptAI)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
            _fptAI = fptAI;
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

                var latestPerSideNeedsCheck = regs
                //.Where(r => r.Status.Equals(ConstantEnum.VerificationStatus.NEED_MANUAL_CHECK))
                .GroupBy(r => r.Side)
                .Select(g => g
                    .OrderByDescending(r => r.CreateDate)
                    .First())
                .ToList();

                if (!latestPerSideNeedsCheck.Any()) throw new KeyNotFoundException("There are no document needs approval!");

                var mapped = new ApproveLicenseView();

                foreach (var r in regs)
                {
                    if (isApproved)
                    {
                        r.Status = ConstantEnum.VerificationStatus.MANUAL_APPROVED;
                        user.IsVerified = true;
                    }
                    else
                    {
                        r.Status = ConstantEnum.VerificationStatus.REJECTED;
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

            var latestPerUserPerSide = result
                .GroupBy(dl => new { dl.UserId, dl.Side })
                .Select(g => g
                    .OrderByDescending(dl => dl.CreateDate)
                    .ThenByDescending(dl => dl.Id)
                    .First())
                .ToList();

            //await _upload.EnsureInitializedAsync();

            var tasks = latestPerUserPerSide.Select(async license =>
            {
                var url = await _upload.CreateSignedUrlAsync(
                    license.Bucket,
                    license.FilePath,
                    expirationTimeinSeconds);

                var view = _mapper.Map<DriverLicenseView>(license);
                view.Urls = new List<string> { url };

                return view;
            });

            var views = (await Task.WhenAll(tasks)).ToList();

            return (null, views);
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

            var latestPerSide = result
                .GroupBy(r => r.Side)
                .Select(g => g
                    .OrderByDescending(r => r.CreateDate).First())
                .ToList();

            //await _upload.EnsureInitializedAsync();

            var tasks = latestPerSide.Select(async license =>
            {
                var url = await _upload.CreateSignedUrlAsync(
                    license.Bucket,
                    license.FilePath,
                    expirationTimeinSeconds);

                var view = _mapper.Map<DriverLicenseView>(license);
                view.Urls = new List<string> { url };

                return view;
            });

            var views = (await Task.WhenAll(tasks)).ToList();

            return (null, views);
        }

        public async Task<List<DriverLicense>> OverwritePrevLicenseImagesAsync(Guid userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //await _upload.EnsureInitializedAsync();
                var userLicense = await _unitOfWork._driverLicenseRepo.GetLicenseByUserIdAsync(userId);

                foreach (var i in userLicense)
                {
                    i.Status = ConstantEnum.Statuses.INACTIVE;
                }
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return userLicense;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<DriverLicenseView> UpdateDriverLicenseAsync(Guid userId, IFormFile frontImage)
        {
            try
            {
                
                var userExist = await _unitOfWork._userRepo.GetFirstWithIncludeAsync(x => x.Id.Equals(userId));

                if (userExist == null)
                {
                    throw new InvalidOperationException("User doesn't exist!");
                }

                await OverwritePrevLicenseImagesAsync(userExist.Id);

                await _unitOfWork.BeginTransactionAsync();
                var uploadTasks = new List<Task<(string url, DriverLicense obj)>>();

                //await _upload.EnsureInitializedAsync();
                uploadTasks.Add(UploadDriverLicenseAsync(frontImage, userId, (int)ConstantEnum.DriverLicenseSide.FrontSide));

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                var aiCheck = await AutoApproveLicenseAsync(frontImage);

                if (aiCheck.Status.Equals(ConstantEnum.VerificationStatus.AUTO_APPROVED))
                    userExist.IsVerified = true;

                //But ef core operation is sequential
                foreach (var u in uploadResults)
                {
                    if (u.obj.Side == 1) _mapper.Map(aiCheck, u.obj);
                    await _unitOfWork._driverLicenseRepo.AddDriverLicenseAsync(u.obj);
                }
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var mapped = new DriverLicenseView
                {
                    Urls = urls,
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Status = aiCheck.Status,
                    Side = aiCheck.Side,
                    LicenseNumber = aiCheck.LicenseNumber,
                    LicenseName = aiCheck.LicenseName,
                    LicenseDoB = aiCheck.LicenseDoB,
                    LicenseClass = aiCheck.LicenseClass,
                    LicenseIssue = aiCheck.LicenseIssue,
                    LicenseExpiry = aiCheck.LicenseExpiry
                };
                return mapped;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string url, DriverLicense obj)> UploadDriverLicenseAsync(IFormFile file, Guid userId, int side)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.DriverLicense;
                string uploadDate = DateTime.UtcNow.AddHours(7).ToString("ddMMyyyy");
                string noExt = Path.GetFileNameWithoutExtension(file.FileName);
                string sideName = "";

                if (side == 1) sideName = "Front";
                if (side == 2) sideName = "Back";
                if (side < 1 || side > 2) throw new InvalidOperationException("Only 1 or 2 for front and back!");

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"image{sideName}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
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
                    Side = side,
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.PENDING,
                    UserId = userId
                };

                return (url, license);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<DriverLicense> AutoApproveLicenseAsync(IFormFile frontImage)
        {
            try
            {
                var AICheck = await _fptAI.ExtractDriverLicenseInfo(frontImage);
                int aiCheckResult = AICheck.CheckValidation();

                //check ai for null

                var license = new DriverLicense();

                if(aiCheckResult == 1)
                {
                    license.Status = ConstantEnum.VerificationStatus.AUTO_APPROVED;
                }
                else if(aiCheckResult == 0)
                {
                    license.Status = ConstantEnum.VerificationStatus.NEED_MANUAL_CHECK;
                }
                else
                {
                    license.Status = ConstantEnum.VerificationStatus.REJECTED;
                }
                license.Side = 1;

                license.LicenseNumber = AICheck.LicenseId;
                license.LicenseName = AICheck.NameOnLicense;
                license.LicenseDoB = ParseDate(AICheck.DateOfBirth);
                license.LicenseClass = AICheck.Class;
                license.LicenseIssue = ParseDate(AICheck.DateOfIssue);
                license.LicenseExpiry = ParseDate(AICheck.DateOfExpiry);

                return license;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static DateOnly? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var formats = new[]
            {
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "yyyy-MM-dd",
                "MM/dd/yyyy"
            };

            if (DateOnly.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return date;
            }

            return null;
        }

    }
}