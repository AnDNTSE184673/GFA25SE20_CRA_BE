using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.ResponseDTO.Feedbacks;
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
            return _mapper.Map<List<FeedbackView>>(result);
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

                string bucket = ConstantEnum.SupabaseBucket.FeedbackImages;
                string uploadDate = DateTime.UtcNow.ToString("ddMMyyyy"); ;

                foreach (var file in form.Medias)
                {
                    string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                    string fileName = $"{file.FileName}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                    string imagePath = $"{ConstantEnum.SupabaseBucket.publicFolder}/{newFeedback.Id}/{fileName}"; //userid/carid_date.ext
                    await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeinSeconds);
                }

                var result = await _unitOfWork._feedbackRepo.CreateFeedbackAsync(newFeedback);

                await _unitOfWork.CommitTransactionAsync();

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var mapped = _mapper.Map<FeedbackView>(result.feedback);
                    return (result.status, mapped);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
