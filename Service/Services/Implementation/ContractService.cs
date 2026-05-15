using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Contract;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.Contract;
using Repository.Extension.SupabaseFileUploader;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Service.Services.Implementation
{
    public class ContractService : IContractService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly UploadFile _upload;

        private int expirationTimeinSeconds = 1800;
        private bool isPublic = false;

        public ContractService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload, int expirationTimeinSeconds, bool isPublic)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
            this.expirationTimeinSeconds = expirationTimeinSeconds;
            this.isPublic = isPublic;
        }

        public async Task<ContractViewDTO> UploadDocuments(ContractUploadDTO form)
        {
            try
            {
                var userAExist = await _unitOfWork._userRepo.GetFirstWithIncludeAsync(x => x.Id.Equals(form.PartyA));
                var userBExist = await _unitOfWork._userRepo.GetFirstWithIncludeAsync(x => x.Id.Equals(form.PartyB));

                if (userAExist == null || userBExist == null)
                {
                    throw new InvalidOperationException("Either party user doesn't exist!");
                }

                await _unitOfWork.BeginTransactionAsync();
                var uploadTasks = new List<Task<(string url, Contract obj)>>();

                foreach (var file in form.Documents)
                {
                    uploadTasks.Add(UploadContractInnerAsync(form, file));
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
                    await _unitOfWork._contractRepo.AddContractAsync(u.obj);
                }
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var returnObj = new ContractViewDTO
                {
                    BookingId = form.BookingId,
                    PartyA = form.PartyA,
                    PartyB = form.PartyB,
                    CreateDate = DateTime.UtcNow,
                    ValidUntil = form.ValidDate,
                    DocUrls = urls
                };

                return returnObj;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<ContractViewDTO> ViewContractsByBooking(Guid bookingId)
        {
            var rows = await _unitOfWork._contractRepo.GetContractsByBookingAsync(bookingId);
            var uploadTasks = new List<Task<string>>();

            //await _upload.EnsureInitializedAsync();

            foreach (var r in rows)
            {
                uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeinSeconds));
            }
            var uploadResults = await Task.WhenAll(uploadTasks);
            return (uploadResults, _mapper.Map<List<ContractViewDTO>>(rows)); //TODO add mapping to Profile
        }

        public async Task<(string url, Contract obj)> UploadContractInnerAsync(ContractUploadDTO form, IFormFile file)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.Contracts;
                string uploadDate = DateTime.UtcNow.AddHours(7).ToString("ddMMyyyy");
                string noExt = Path.GetFileNameWithoutExtension(file.FileName);
                string sideName = "";

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"contract_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                string imagePath = $"{form.BookingId}/{fileName}"; //userid/carid_date.ext

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeinSeconds, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var license = new Contract
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.PENDING,
                    ValidUntil = form.ValidDate,
                    TermsDetail = form.TermDetails,
                    PartyAId = form.PartyA, 
                    PartyBId = form.PartyB, 
                    BookingId = form.BookingId 
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
