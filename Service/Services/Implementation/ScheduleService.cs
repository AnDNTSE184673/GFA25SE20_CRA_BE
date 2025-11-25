using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Schedule;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRentalRate;
using Repository.DTO.ResponseDTO.Schedule;
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

        public ScheduleService(IMapper mapper, UnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ScheduleView> StatusChangeAsync(Guid bookingId, bool isCompleted, bool isOverdue)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var schedule = await _unitOfWork._scheduleRepo.GetByIdWithIncludeAsync(bookingId, "Id", x => x.User, x => x.Car, x => x.Booking);

                if (schedule == null) throw new KeyNotFoundException("Car not found");

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

                var result1 = await _unitOfWork._scheduleRepo.CreateScheduleAsync(schedule);

                await _unitOfWork.CommitTransactionAsync();

                if (result1.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1.Schedules);
                    return returnObj;
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

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

        public async Task<(string status, ScheduleView view)> CheckInAsync(Guid userId, Guid carId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork._bookingRepo.GetUnfinishedLatestBookingFromCarAndCustomer(userId, carId);

                if (booking == null) throw new KeyNotFoundException("Booking not found");

                var user = await _unitOfWork._userRepo.GetByIdAsync(booking.UserId);
                
                var car = await _unitOfWork._carRepo.GetByIdAsync(booking.CarId);

                if (car == null || booking == null) throw new KeyNotFoundException("Car and user related to the booking not found");

                var oldSchedules = await _unitOfWork._scheduleRepo.GetLastScheduleByBookingAndType(booking.Id, ConstantEnum.ScheduleTypeConstants.Pickup);

                oldSchedules.Status = ConstantEnum.Statuses.COMPLETED;

                await _unitOfWork._scheduleRepo.UpdateScheduleAsync(oldSchedules);

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
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ScheduleView view)> CheckOutAsync(Guid userId, Guid carId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var booking = await _unitOfWork._bookingRepo.GetUnfinishedLatestBookingFromCarAndCustomer(userId, carId);

                if (booking == null) throw new KeyNotFoundException("Booking not found");

                var user = await _unitOfWork._userRepo.GetByIdAsync(booking.UserId);

                var car = await _unitOfWork._carRepo.GetByIdAsync(booking.CarId);

                if (car == null || booking == null) throw new KeyNotFoundException("Car and user related to the booking not found");

                var oldSchedules = await _unitOfWork._scheduleRepo.GetLastScheduleByBookingAndType(booking.Id, ConstantEnum.ScheduleTypeConstants.Return);

                oldSchedules.Status = ConstantEnum.Statuses.COMPLETED;

                var result1 = await _unitOfWork._scheduleRepo.UpdateScheduleAsync(oldSchedules);

                await _unitOfWork.CommitTransactionAsync();

                if (result1 == null)
                {
                    throw new Exception("Create function failed to create the object!");
                }
                else
                {
                    var returnObj = _mapper.Map<ScheduleView>(result1);
                    return (ConstantEnum.RepoStatus.SUCCESS, returnObj);
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
                mapped.Id = Guid.NewGuid();
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
                mapped.Id = Guid.NewGuid();
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
    }
}
