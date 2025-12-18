using AutoMapper;
using Medo;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
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

        public async Task<string> DeleteCarInquiry(Guid id)
        {
            try
            {
                var exist = await _unitOfWork._inquiryRepo.GetByIdWithIncludeAsync(id, "Id", x => x.InquiryImages);

                if (exist == null) throw new KeyNotFoundException("Inquiry not found!");

                var convos = await _unitOfWork._inquiryRepo.GetAllConversationsBetween2Users(exist.SenderId, exist.ReceiverId);

                var checkParent = convos.Where(x => x.ParentInquiryId.Equals(exist.Id)).ToList(); //whether it is the parent of another msg

                await _unitOfWork.BeginTransactionAsync();

                foreach (var msg in checkParent)
                {
                    msg.ParentInquiryId = exist.ParentInquiryId;
                    await _unitOfWork._inquiryRepo.UpdateInquiryAsync(msg);
                }

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

        public async Task<List<InquiryView>> GetAllUniqueInquiriesByUser(Guid userId)
        {
            var result = await _unitOfWork._inquiryRepo.GetUserConversations(userId);
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

        //begin a chat
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
                newInquiry.Id = Uuid7.NewGuid();
                newInquiry.CreateDate = newInquiry.UpdateDate = DateTime.UtcNow;
                newInquiry.Type = ConstantEnum.InquiryTypeConstants.Question; //redundant atm
                newInquiry.Status = ConstantEnum.InquiryStatusConstants.Open;
                
                newInquiry.ParentInquiryId = null;

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


        public async Task<(string status, InquiryView view)> AnswerCarInquiry(AnswerInquiryForm form)
        {
            try
            {
                var userSendExist = await _unitOfWork._userRepo.GetByIdAsync(form.SenderId);
                var userReceiveExist = await _unitOfWork._userRepo.GetByIdAsync(form.ReceiverId);

                if (userSendExist == null || userReceiveExist == null)
                {
                    throw new InvalidOperationException("Either or both user of inquiry doesn't exist!");
                }

                var newInquiry = _mapper.Map<Inquiry>(form);
                newInquiry.Id = Uuid7.NewGuid();
                newInquiry.CreateDate = newInquiry.UpdateDate = DateTime.UtcNow;
                newInquiry.Type = ConstantEnum.InquiryTypeConstants.Answer; //redundant atm

                if (form.isOpen) newInquiry.Status = ConstantEnum.InquiryStatusConstants.Open;
                else newInquiry.Status = ConstantEnum.InquiryStatusConstants.Closed;

                //else find root from user FKs
                var rootExist = await _unitOfWork._inquiryRepo.GetRootInquiryByBothUser(form.SenderId, form.ReceiverId);
                List<Inquiry> conversation = new List<Inquiry>();
                if (rootExist != null) conversation = await _unitOfWork._inquiryRepo.GetInquiryTreeFromRoot(rootExist.Id);
                bool insertIntoMiddleOfLinked = false;

                if (rootExist == null)
                {
                    //set this as root
                    newInquiry.ParentInquiryId = null;
                }
                else
                {
                    //If form has parentId, use it
                    //Else find last msg in chain and continue from there
                    if (form.ParentInquiryId != null)
                    {
                        var parent = await _unitOfWork._inquiryRepo.GetByIdAsync(form.ParentInquiryId.Value);

                        if (parent == null) throw new Exception("Invalid ParentInquiryId");
                        
                        //change this check correct chat logic a bit
                        var checkCorrectChat = conversation.Where(x => x.ParentInquiryId.Equals(form.ParentInquiryId) || x.Id.Equals(form.ParentInquiryId));
                        if (!checkCorrectChat.Any()) throw new Exception("This parent doesn't belong in this conversation!");
                        else
                        {
                            var nextChatofParentCheck = conversation.Where(x => x.ParentInquiryId.Equals(form.ParentInquiryId)).FirstOrDefault();
                            if (nextChatofParentCheck != null) insertIntoMiddleOfLinked = true;
                            newInquiry.ParentInquiryId = form.ParentInquiryId; 
                        }
                    }
                    else 
                    {
                        var last = conversation
                            .OrderBy(x => x.CreateDate)
                            .LastOrDefault(); //get newest
                        newInquiry.ParentInquiryId = last.Id;
                    }
                }
                await _unitOfWork.BeginTransactionAsync();

                var result = await _unitOfWork._inquiryRepo.CreateInquiryAsync(newInquiry);

                //only do this if the new Inquiry is to be inserted into the middle of the list
                if (insertIntoMiddleOfLinked)
                {
                    var nextChatofParent = conversation.Where(x => x.ParentInquiryId.Equals(form.ParentInquiryId)).FirstOrDefault();
                    nextChatofParent.ParentInquiryId = newInquiry.Id;
                    await _unitOfWork._inquiryRepo.UpdateAsync(nextChatofParent);
                }

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
                string uploadDate = DateTime.UtcNow.AddHours(7).ToString("ddMMyyyy");
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

        public async Task<List<InquiryView>> GetInquiryTreeByBothSides(Guid senderId, Guid receiverId)
        {
            var root = await _unitOfWork._inquiryRepo.GetRootInquiryByBothUser(senderId, receiverId);

            if (root == null) throw new KeyNotFoundException("These users has no prior conversation yet!");

            var tree = await _unitOfWork._inquiryRepo.GetInquiryTreeFromRoot(root.Id);

            var inquiryViews = new List<InquiryView>();
            foreach (var inquiry in tree)
            {
                //Task.WhenAll is to run all the url getting at once
                var urls = await Task.WhenAll(inquiry.InquiryImages.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));

                var view = _mapper.Map<InquiryView>(inquiry);

                view.ImageUrls = urls.ToList();

                inquiryViews.Add(view);
            }
            return inquiryViews.OrderByDescending(x => x.CreateDate).ToList();
        }
    }
}
