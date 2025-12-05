using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Inquiry;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.DTO.ResponseDTO.Inquiry;
using Repository.Extension.SupabaseFileUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class InquiryService : IInquiryService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UploadFile _upload;

        int expirationTimeinSeconds = 1800;
        bool isPublic = true;

        public InquiryService(UnitOfWork unitOfWork, IMapper mapper, UploadFile upload)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _upload = upload;
        }

        public Task<string> DeleteCarInquiry(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<InquiryView> EditCarInquiry(Guid id, EditInquiryForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = _unitOfWork._inquiryRepo.GetById(id);
                if (existing == null)
                    return null;

                //partial mapping
                var inquiry = _mapper.Map(form, existing);
                inquiry.UpdateDate = DateTime.UtcNow;
                var result = await _unitOfWork._inquiryRepo.UpdateInquiryAsync(inquiry);

                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<InquiryView>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<InquiryView>> GetAllUniqueInquiriesBySender(Guid customerId)
        {
            var result = await _unitOfWork._inquiryRepo.GetInquiryByReceiver(receiverId);
            var inquiryViews = new List<InquiryView>();
            foreach (var inquiry in result)
            {
                //Task.WhenAll is to run all the url getting at once
                var urls = await Task.WhenAll(inquiry.InquiryImages.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));

                var view = _mapper.Map<InquiryView>(inquiry);

                view.ImageUrls = urls.ToList();

                inquiryViews.Add(view);
            }
            return inquiryViews;
        }

        public async Task<List<InquiryView>> GetInquiriesByReceiver(Guid receiverId)
        {
            var result = await _unitOfWork._inquiryRepo.GetInquiryByReceiver(receiverId);
            var inquiryViews = new List<InquiryView>();
            foreach (var inquiry in result)
            {
                //Task.WhenAll is to run all the url getting at once
                var urls = await Task.WhenAll(inquiry.InquiryImages.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));

                var view = _mapper.Map<InquiryView>(inquiry);

                view.ImageUrls = urls.ToList();

                inquiryViews.Add(view);
            }
            return inquiryViews;
        }

        public async Task<List<InquiryView>> GetInquiriesBySender(Guid senderId)
        {
            var result = await _unitOfWork._inquiryRepo.GetInquiryBySender(senderId);
            var inquiryViews = new List<InquiryView>();
            foreach (var inquiry in result)
            {
                //Task.WhenAll is to run all the url getting at once
                var urls = await Task.WhenAll(inquiry.InquiryImages.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));

                var view = _mapper.Map<InquiryView>(inquiry);

                view.ImageUrls = urls.ToList();

                inquiryViews.Add(view);
            }
            return inquiryViews;
        }

        public async Task<(string status, InquiryView view)> LeaveCarInquiry(CreateInquiryForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var userSendExist = await _unitOfWork._userRepo.GetByIdAsync(form.SenderId);
                var userReceiveExist = await _unitOfWork._userRepo.GetByIdAsync(form.ReceiverId);

                if (userSendExist == null || userReceiveExist == null)
                {
                    throw new InvalidOperationException("Either or both user of inquiry doesn't exist!");
                }

                var newInquiry = _mapper.Map<Inquiry>(form);
                newInquiry.Id = Guid.NewGuid();
                newInquiry.CreateDate = newInquiry.UpdateDate = DateTime.UtcNow;
                newInquiry.Type = ConstantEnum.InquiryTypeConstants.Question; //customer side
                if(form.isOpen) newInquiry.Status = ConstantEnum.InquiryStatusConstants.Open;
                else newInquiry.Status = ConstantEnum.InquiryStatusConstants.Closed;

                //If frontend provides a ParentInquiryId → use it (valid reply)
                if (form.ParentInquiryId != null)
                {
                    var parent = await _unitOfWork._inquiryRepo.GetByIdAsync(form.ParentInquiryId.Value);

                    if (parent == null)
                        throw new Exception("Invalid ParentInquiryId");

                    //attach parent
                    newInquiry.ParentInquiryId = parent.Id;
                }

                //If ParentInquiryId is null, we need to find the thread root
                var rootExist = await _unitOfWork._inquiryRepo.GetRootInquiryByBothUser(form.SenderId, form.ReceiverId);

                if (rootExist == null)
                {
                    //set this as root
                    newInquiry.ParentInquiryId = null;
                }
                else
                {
                    //find last msg in chain and continue from there
                    var conversation = await _unitOfWork._inquiryRepo.GetInquiryTreeFromRoot(rootExist.Id);
                    var last = conversation.LastOrDefault(); //get newest
                    newInquiry.ParentInquiryId = last.Id;
                }

                var result = await _unitOfWork._inquiryRepo.CreateInquiryAsync(newInquiry);

                //File IO is parallel
                var uploadTasks = new List<Task<(string url, InquiryImages obj)>>();

                foreach (var file in form.Medias)
                {
                    uploadTasks.Add(UploadInquiryImagesAsync(file, newInquiry.Id));
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
                    await _unitOfWork._inquiryImageRepo.AddInquiryImagesAsync(u.obj);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var mapped = _mapper.Map<InquiryView>(result.obj);
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


        public async Task<(string status, InquiryView view)> AnswerCarInquiry(CreateInquiryForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var userSendExist = await _unitOfWork._userRepo.GetByIdAsync(form.SenderId);
                var userReceiveExist = await _unitOfWork._userRepo.GetByIdAsync(form.ReceiverId);

                if (userSendExist == null || userReceiveExist == null)
                {
                    throw new InvalidOperationException("Either or both user of inquiry doesn't exist!");
                }

                var newInquiry = _mapper.Map<Inquiry>(form);
                newInquiry.Id = Guid.NewGuid();
                newInquiry.CreateDate = newInquiry.UpdateDate = DateTime.UtcNow;
                newInquiry.Type = ConstantEnum.InquiryTypeConstants.Answer; //car owner side

                if (form.isOpen) newInquiry.Status = ConstantEnum.InquiryStatusConstants.Open;
                else newInquiry.Status = ConstantEnum.InquiryStatusConstants.Closed;

                //If form has parentId, use it
                if (form.ParentInquiryId != null)
                {
                    var parent = await _unitOfWork._inquiryRepo.GetByIdAsync(form.ParentInquiryId.Value);

                    if (parent == null)
                        throw new Exception("Invalid ParentInquiryId");

                    //attach parent
                    newInquiry.ParentInquiryId = parent.Id;
                }

                //else find root from user FKs
                var rootExist = await _unitOfWork._inquiryRepo.GetRootInquiryByBothUser(form.SenderId, form.ReceiverId);

                if (rootExist == null)
                {
                    //set this as root
                    newInquiry.ParentInquiryId = null;
                }
                else
                {
                    //find last msg in chain and continue from there
                    var conversation = await _unitOfWork._inquiryRepo.GetInquiryTreeFromRoot(rootExist.Id);
                    var last = conversation.LastOrDefault(); //get newest
                    newInquiry.ParentInquiryId = last.Id;
                }

                var result = await _unitOfWork._inquiryRepo.CreateInquiryAsync(newInquiry);

                //File IO is parallel
                var uploadTasks = new List<Task<(string url, InquiryImages obj)>>();

                foreach (var file in form.Medias)
                {
                    uploadTasks.Add(UploadInquiryImagesAsync(file, newInquiry.Id));
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
                    await _unitOfWork._inquiryImageRepo.AddInquiryImagesAsync(u.obj);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var mapped = _mapper.Map<InquiryView>(result.obj);
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

        public async Task<(string url, InquiryImages obj)> UploadInquiryImagesAsync(IFormFile file, Guid inquiryId)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.InquiryImages;
                string uploadDate = DateTime.UtcNow.ToString("ddMMyyyy");
                string noExt = Path.GetFileNameWithoutExtension(file.FileName);

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"{noExt}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                string imagePath = $"{ConstantEnum.SupabaseBucket.publicFolder}/{inquiryId}/{fileName}"; //userid/carid_date.ext

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeinSeconds, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var inquiryImage = new InquiryImages
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.ACTIVE,
                    InquiryId = inquiryId
                };

                return (url, inquiryImage);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
