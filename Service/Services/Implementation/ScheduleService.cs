using AutoMapper;
using Medo;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using PayOS.Exceptions;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.Schedule;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.CarRentalRate;
using Repository.DTO.ResponseDTO.Schedule;
using Repository.Extension.SupabaseFileUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class ScheduleService : IScheduleService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly UploadFile _upload;

        int expirationTimeSec = 1800;
        bool isPublic = false;

        public ScheduleService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
        }

        public async Task<ScheduleView> StatusChangeAsync(Guid scheduleId, bool isCompleted, bool isOverdue)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var schedule = await _unitOfWork._scheduleRepo.GetByIdWithIncludeAsync(scheduleId, "Id", x => x.User, x => x.Car, x => x.Booking);

                if (schedule == null) throw new KeyNotFoundException("Schedule not found");

                if (isCompleted)
                {
                    schedule.Status = ConstantEnum.Statuses.COMPLETED;
                    if (schedule.IsBlocking)
                    {
                        var car = await _unitOfWork._carRepo.GetByIdAsync(schedule.CarId);
                        if (car.Status.Equals(ConstantEnum.Statuses.INACTIVE))
                        {
                            car.Status = ConstantEnum.Statuses.ACTIVE;
                            await _unitOfWork._carRepo.UpdateCarAsync(car);
                        }
                    }
                }
                else
                {
                    schedule.Status = ConstantEnum.Statuses.ACTIVE;
                    if (schedule.IsBlocking)
                    {
                        var car = await _unitOfWork._carRepo.GetByIdAsync(schedule.CarId);
                        if (car.Status.Equals(ConstantEnum.Statuses.ACTIVE))
                        {
                            car.Status = ConstantEnum.Statuses.INACTIVE;
                            await _unitOfWork._carRepo.UpdateCarAsync(car);
                        }
                    }
                }

                var result1 = await _unitOfWork._scheduleRepo.UpdateScheduleAsync(schedule); //update

                await _unitOfWork.CommitTransactionAsync();

                if (result1 == null)
                {
                    throw new Exception("Update failed!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1);
                    return returnObj;
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        //COnsider giving this carHandover too and another method to clear car out of maintenance and track location
        public async Task<(string status, ScheduleView view)> SetMaintenanceAsync(MaintenanceSchedule form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();


                var car = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);

                if (car == null) throw new KeyNotFoundException("Car not found");

                var newSchedules = new CreateScheduleForm
                {
                    Title = form.Title.IsNullOrEmpty() ? ConstantEnum.ScheduleDefaultTitle.MAINTENANCE : form.Title,
                    Location = form.Location,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    ScheduleType = ConstantEnum.ScheduleTypeConstants.Maintenance,
                    Priority = 1,
                    Note = form.Note,
                    IsBlocking = true, //Car is not available for rent atm
                    CarId = form.CarId,
                    UserId = null,
                    BookingId = null
                };

                var mapped = _mapper.Map<Schedules>(newSchedules);
                mapped.Status = ConstantEnum.Statuses.ACTIVE;
                var result1 = await _unitOfWork._scheduleRepo.CreateScheduleAsync(mapped);

                car.Status = ConstantEnum.Statuses.INACTIVE;
                await _unitOfWork._carRepo.UpdateCarAsync(car);

                await _unitOfWork.CommitTransactionAsync();

                if (result1.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1.Schedules);
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ScheduleView view, CICOImageView image)> CheckInAsync(CICOForm form, string userAgent)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(form.BookingId);
                if (booking == null) throw new KeyNotFoundException("Ongoing booking not found");

                var user = await _unitOfWork._userRepo.GetByIdAsync(booking.UserId);
                var car = await _unitOfWork._carRepo.GetByIdAsync(booking.CarId);
                if (car == null || booking == null) throw new KeyNotFoundException("Car and user related to the booking not found");
               
                var oldSchedules = await _unitOfWork._scheduleRepo.GetLastScheduleByBookingAndType(booking.Id, ConstantEnum.ScheduleTypeConstants.Pickup);
                if(oldSchedules == null) throw new KeyNotFoundException("Schedules for pick up not found");
                oldSchedules.Status = ConstantEnum.Statuses.COMPLETED;

                await _unitOfWork._scheduleRepo.UpdateScheduleAsync(oldSchedules);

                var imageResult = await UploadImageWhenCheckInOutInnerService(booking.Id, form.images, true);

                var newCarHandover = new CarHandoverAudit
                {
                    Id = Guid.NewGuid(),
                    Type = ConstantEnum.ScheduleTypeConstants.Pickup,
                    Description = form.Description,
                    ScheduleId = oldSchedules.Id,
                    ResponsibleStaffId = form.ResponsibleStaffId,
                    Location = form.Location
                };

                var handoverResult = await _unitOfWork._carHandoverRepo.CreateCarHandoverAsync(newCarHandover);
                if (handoverResult.status.Equals(ConstantEnum.RepoStatus.FAILURE)) throw new Exception("Creation of car handover log has failed!");

                var newStaffLog = new StaffLogAudit
                {
                    Id = Guid.NewGuid(),
                    Action = $"Schedules Check-in",
                    UserAgent = userAgent,
                    RelatedHandoverId = newCarHandover.Id,
                    StaffId = form.ResponsibleStaffId
                };

                var staffLogResult = await _unitOfWork._staffLogRepo.CreateStaffLogAsync(newStaffLog);
                if (staffLogResult.status.Equals(ConstantEnum.RepoStatus.FAILURE)) throw new Exception("Creation of car handover log has failed!");

                var newSchedules = new CreateScheduleForm
                {
                    Title = ConstantEnum.ScheduleDefaultTitle.RETURN,
                    Location = booking.DropoffPlace,
                    StartDate = booking.DropoffTime,
                    EndDate = booking.DropoffTime,
                    ScheduleType = ConstantEnum.ScheduleTypeConstants.Return,
                    Priority = 1,
                    Note = "",
                    IsBlocking = false, //Car is not available for rent atm
                    CarId = booking.CarId,
                    UserId = booking.UserId,
                    BookingId = booking.Id
                };

                var mapped = _mapper.Map<Schedules>(newSchedules);
                mapped.Status = ConstantEnum.Statuses.ACTIVE;
                mapped.CreateDate = mapped.UpdateDate = DateTime.UtcNow;
                var result1 = await _unitOfWork._scheduleRepo.CreateScheduleAsync(mapped);

                car.Status = ConstantEnum.Statuses.ACTIVE;
                await _unitOfWork._carRepo.UpdateCarAsync(car);

                await _unitOfWork.CommitTransactionAsync();

                if (result1.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1.Schedules);
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj, imageResult.regDoc);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ScheduleView view, CICOImageView image)> CheckOutAsync(CICOForm form, string userAgent)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(form.BookingId);
                if (booking == null) throw new KeyNotFoundException("Ongoing booking not found");

                var user = await _unitOfWork._userRepo.GetByIdAsync(booking.UserId);
                var car = await _unitOfWork._carRepo.GetByIdAsync(booking.CarId);
                if (car == null || booking == null) throw new KeyNotFoundException("Car and user related to the booking not found");
                
                var oldSchedules = await _unitOfWork._scheduleRepo.GetLastScheduleByBookingAndType(booking.Id, ConstantEnum.ScheduleTypeConstants.Return);
                if (oldSchedules == null) throw new KeyNotFoundException("Schedules for pick up not found");
                oldSchedules.Status = ConstantEnum.Statuses.COMPLETED;

                var result1 = await _unitOfWork._scheduleRepo.UpdateScheduleAsync(oldSchedules);

                var imageResult = await UploadImageWhenCheckInOutInnerService(booking.Id, form.images, false);

                var newCarHandover = new CarHandoverAudit
                {
                    Id = Guid.NewGuid(),
                    Type = ConstantEnum.ScheduleTypeConstants.Return,
                    Description = form.Description,
                    ScheduleId = oldSchedules.Id,
                    ResponsibleStaffId = form.ResponsibleStaffId,
                    Location = form.Location
                };

                var handoverResult = await _unitOfWork._carHandoverRepo.CreateCarHandoverAsync(newCarHandover);
                if (handoverResult.status.Equals(ConstantEnum.RepoStatus.FAILURE)) throw new Exception("Creation of car handover log has failed!");

                var newStaffLog = new StaffLogAudit
                {
                    Id = Guid.NewGuid(),
                    Action = $"Schedules Check-out",
                    UserAgent = userAgent,
                    RelatedHandoverId = newCarHandover.Id,
                    StaffId = form.ResponsibleStaffId
                };

                var staffLogResult = await _unitOfWork._staffLogRepo.CreateStaffLogAsync(newStaffLog);
                if (staffLogResult.status.Equals(ConstantEnum.RepoStatus.FAILURE)) throw new Exception("Creation of car handover log has failed!");

                await _unitOfWork.CommitTransactionAsync();

                if (result1 == null)
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1);
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj, imageResult.regDoc);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ScheduleView>> GetAllSchedulesOfBookingAsync(Guid bookingId)
        {
            var result = await _unitOfWork._scheduleRepo.GetSchedulesByBooking(bookingId);
            return _mapper.Map<List<ScheduleView>>(result);
        }

        public async Task<List<ScheduleView>> GetAllSchedulesOfCarAsync(Guid carId)
        {
            var result = await _unitOfWork._scheduleRepo.GetSchedulesByCar(carId);
            return _mapper.Map<List<ScheduleView>>(result);
        }

        public async Task<List<ScheduleView>> GetAllSchedulesOfUserAsync(Guid userId)
        {
            var result = await _unitOfWork._scheduleRepo.GetSchedulesByUser(userId);
            return _mapper.Map<List<ScheduleView>>(result);
        }

        public async Task<string> RemoveSchedulesAsync(Guid scheduleId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var result = await _unitOfWork._scheduleRepo.DeleteScheduleAsync(scheduleId);

                await _unitOfWork.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ScheduleView view)> SetCarSchedulesAsync(CreateScheduleForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = new User();
                var booking = new Booking();
                var car = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);
                if (car == null) throw new KeyNotFoundException("Car not found!");
                if (form.UserId.HasValue) user = await _unitOfWork._userRepo.GetByIdAsync(form.UserId.Value);
                if (form.BookingId.HasValue) booking = await _unitOfWork._bookingRepo.GetByIdAsync(form.BookingId.Value);

                var mapped = _mapper.Map<Schedules>(form);
                mapped.Id = Uuid7.NewGuid();
                mapped.CreateDate = DateTime.UtcNow;
                mapped.UpdateDate = DateTime.UtcNow;
                mapped.Status = ConstantEnum.Statuses.ACTIVE;

                var result1 = await _unitOfWork._scheduleRepo.CreateScheduleAsync(mapped);
                
                await _unitOfWork.CommitTransactionAsync();

                if (result1.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1.Schedules);
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<ScheduleView> UpdateCarSchedulesAsync(UpdateScheduleForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var exist = await _unitOfWork._scheduleRepo.GetByIdAsync(form.Id);
                var car = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);
                if (car == null) throw new KeyNotFoundException("Car not found!");
                if (exist == null) throw new KeyNotFoundException("No schedule with that Id found!");

                var mapped = _mapper.Map(form, exist);
                
                mapped.UpdateDate = DateTime.UtcNow;

                var result1 = await _unitOfWork._scheduleRepo.UpdateScheduleAsync(mapped);

                await _unitOfWork.CommitTransactionAsync();

                var returnObj = _mapper.Map<ScheduleView>(result1);
                return returnObj;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ScheduleView view)> SetCarSchedulesInnerServiceAsync(CreateScheduleForm form)
        {
            try
            {
                var user = new User();
                var booking = new Booking();
                var car = await _unitOfWork._carRepo.GetByIdAsync(form.CarId);
                if (car == null) throw new KeyNotFoundException("Car not found!");
                if (form.UserId.HasValue) user = await _unitOfWork._userRepo.GetByIdAsync(form.UserId.Value);
                if (form.BookingId.HasValue) booking = await _unitOfWork._bookingRepo.GetByIdAsync(form.BookingId.Value);

                var mapped = _mapper.Map<Schedules>(form);
                mapped.Id = Uuid7.NewGuid();
                mapped.CreateDate = DateTime.UtcNow;
                mapped.UpdateDate = DateTime.UtcNow;
                mapped.Status = ConstantEnum.Statuses.ACTIVE;

                var result1 = await _unitOfWork._scheduleRepo.CreateScheduleAsync(mapped);

                if (result1.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1.Schedules);
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, CICOImageView regDoc)> UploadImageWhenCheckInOutInnerService(Guid bookingId, List<IFormFile> images, bool isCheckIn)
        {
            var bucket = ConstantEnum.SupabaseBucket.CheckInOutImages;
            try
            {
                string folder = "";

                if (isCheckIn) folder = "CheckIn";
                else folder = "CheckOut";
                int count = 1;
                var uploadTasks = new List<Task<(string url, ScheduleImage obj)>>();
                //await _upload.EnsureInitializedAsync();
                foreach (var file in images)
                {
                    uploadTasks.Add(UploadCICOImagesAsync(file, bookingId, count, folder, isCheckIn));
                    count++;
                }

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                foreach (var u in uploadResults)
                {
                    await _unitOfWork._scheduleRepo.AddScheduleImages(u.obj);
                }

                var result = await _unitOfWork.SaveChangesAsync();

                var view = new CICOImageView
                {
                    BookingId = bookingId,
                    Urls = urls,
                    CreateDate = DateTime.UtcNow,
                    Status = ConstantEnum.Statuses.PENDING
                };
                return (ConstantEnum.RepoStatus.SUCCESS, view);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string url, ScheduleImage obj)> UploadCICOImagesAsync(IFormFile file, Guid bookingId, int count, string folder, bool isCheckIn)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.CheckInOutImages;
                string uploadDate = DateTime.UtcNow.AddHours(7).ToString("ddMMyyyy");

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"image{count}_{uploadDate}{originalExt}";
                string imagePath = $"{folder}/{bookingId}/{fileName}";

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeSec, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var image = new ScheduleImage
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.PENDING,
                    BookingId = bookingId,
                    IsCheckIn = isCheckIn,
                    IsCheckOut = !isCheckIn,
                };

                return (url, image);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string[] signedUrl, CICOImageView view)> GetCICOImageByBooking(CICOImageSearch form)
        {
            try
            {
                var rows = await _unitOfWork._scheduleRepo.GetScheduleImageByBookingAndState(form.BookingId, form.isCheckIn);

                string type = "";
                if (form.isCheckIn) type = ConstantEnum.ScheduleTypeConstants.Pickup;
                else type = ConstantEnum.ScheduleTypeConstants.Return;

                var schedule = await _unitOfWork._scheduleRepo.GetLastScheduleByBookingAndType(form.BookingId, type);

                var rows2 = await _unitOfWork._carHandoverRepo.GetCarHandoverByScheduleAsync(schedule.Id);

                var uploadTasks = new List<Task<string>>();
                //await _upload.EnsureInitializedAsync();
                foreach (var r in rows)
                {
                    uploadTasks.Add(_upload.CreateSignedUrlAsync(r.Bucket, r.FilePath, expirationTimeSec));
                }

                var mapped = new CICOImageView
                {
                    BookingId = form.BookingId,
                    Description = rows2 != null ? rows2.Description : "There is no Car Handover Audit with this",
                    CreateDate = rows.FirstOrDefault().CreateDate,
                    Status = rows.FirstOrDefault().Status
                };

                try 
                { 
                    var uploadResults = await Task.WhenAll(uploadTasks);
                    mapped.Urls = uploadResults.ToList();
                    return (uploadResults, mapped);
                }
                catch { return (null, mapped); } 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, CICOImageView regDoc)> UploadImageWhenCheckInOut(CheckInOutImages form)
        {
            var bucket = ConstantEnum.SupabaseBucket.CheckInOutImages;
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                string folder = "";
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(form.bookingId);

                if (booking == null) throw new NotFoundException("Booking not found!");

                if (form.isCheckIn) folder = "CheckIn";
                else folder = "CheckOut";
                int count = 1;
                var uploadTasks = new List<Task<(string url, ScheduleImage obj)>>();
                //await _upload.EnsureInitializedAsync();
                foreach (var file in form.images)
                {
                    uploadTasks.Add(UploadCICOImagesAsync(file, form.bookingId, count, folder, form.isCheckIn));
                    count++;
                }

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                foreach (var u in uploadResults)
                {
                    await _unitOfWork._scheduleRepo.AddScheduleImages(u.obj);
                }

                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var view = new CICOImageView
                {
                    BookingId = form.bookingId,
                    Urls = urls,
                    CreateDate = DateTime.UtcNow,
                    Status = ConstantEnum.Statuses.PENDING
                };
                return (ConstantEnum.RepoStatus.SUCCESS, view);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
