using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.Extension.SupabaseFileUploader;
using Supabase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly UploadFile _upload;

        int expirationTimeinSeconds = 1800;
        bool isPublic = true;

        public FeedbackService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
        }

        public async Task<string> DeleteCarFeedback(Guid id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var result = await _unitOfWork._feedbackRepo.DeleteFeedbackAsync(id);

                await _unitOfWork.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<FeedbackView> EditCarFeedback(Guid id, EditFeedbackForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = _unitOfWork._feedbackRepo.GetById(id);
                if (existing == null)
                    return null;

                //partial mapping
                var post = _mapper.Map(form, existing);
                var result = await _unitOfWork._feedbackRepo.UpdateFeedbackAsync(post);

                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<FeedbackView>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<FeedbackView>> GetCarFeedbacks(Guid carId)
        {
            var result = await _unitOfWork._feedbackRepo.GetFeedbacksByCar(carId);
            var feedbackViews = new List<FeedbackView>();
            foreach (var feedback in result)
            {
                //Task.WhenAll is to run all the url getting at once
                var urls = await Task.WhenAll(feedback.FeedbackImages.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));

                var feedbackView = _mapper.Map<FeedbackView>(feedback);

                feedbackView.ImageUrls = urls.ToList();

                feedbackViews.Add(feedbackView);
            }
            return feedbackViews;
        }

        public async Task<(string status, FeedbackView view)> LeaveCarFeedback(CreateFeedbackForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var carExist = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);

                if(carExist == null)
                {
                    throw new InvalidOperationException("Car doesn't exist!");
                }

                var newFeedback = _mapper.Map<Feedback>(form);
                newFeedback.Id = Guid.NewGuid();
                newFeedback.CreateDate = DateTime.UtcNow;

                var result = await _unitOfWork._feedbackRepo.CreateFeedbackAsync(newFeedback);
                result.feedback.Car = carExist;

                List<string> urls = new List<string>();
                foreach (var file in form.Medias)
                {
                    var url = await UploadFeedbackImagesAsync(file, newFeedback.Id);
                    if(url.url.IsNullOrEmpty() && url.obj == null) throw new Exception("File upload failure!");
                    urls.Add(url.url);
                }     

                await _unitOfWork.CommitTransactionAsync();

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var mapped = _mapper.Map<FeedbackView>(result.feedback);
                    mapped.ImageUrls.AddRange(urls);
                    return (result.status, mapped);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, string url, FeedbackImage obj)> UploadFeedbackImagesAsync(IFormFile file, Guid feedbackId)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.FeedbackImages;
                string uploadDate = DateTime.UtcNow.ToString("ddMMyyyy");
                string noExt = Path.GetFileNameWithoutExtension(file.FileName);

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"{noExt}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                string imagePath = $"{ConstantEnum.SupabaseBucket.publicFolder}/{feedbackId}/{fileName}"; //userid/carid_date.ext

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeinSeconds, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var feedbackImage = new FeedbackImage
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.ACTIVE,
                    FeedbackId = feedbackId
                };

                var result = await _unitOfWork._feedbackImageRepo.CreateFeedbackImageAsync(feedbackImage);

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null, null);
                }
                else
                {
                    return (result.status, url, result.fbImage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
