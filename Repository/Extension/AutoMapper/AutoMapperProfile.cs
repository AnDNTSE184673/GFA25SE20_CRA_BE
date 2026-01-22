using AutoMapper;
using Microsoft.Extensions.Hosting;
using Repositories.DTO.ResponseDTO.User;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.CarRentalRate;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.RequestDTO.Inquiry;
using Repository.DTO.RequestDTO.ParkingLot;
using Repository.DTO.RequestDTO.Report;
using Repository.DTO.RequestDTO.Schedule;
using Repository.DTO.ResponseDTO;
using Repository.DTO.ResponseDTO.Audits;
using Repository.DTO.ResponseDTO.Booking;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.CarRentalRate;
using Repository.DTO.ResponseDTO.CarToll;
using Repository.DTO.ResponseDTO.DriverLicense;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.DTO.ResponseDTO.GPS;
using Repository.DTO.ResponseDTO.Inquiry;
using Repository.DTO.ResponseDTO.Invoice;
using Repository.DTO.ResponseDTO.ParkingLot;
using Repository.DTO.ResponseDTO.Payment;
using Repository.DTO.ResponseDTO.Report;
using Repository.DTO.ResponseDTO.Schedule;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            #region Automapper 101:
            /*
            CreateMap<[FROM src],[TO dest]>() 
            // e.g from User to UserView
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name)) 
            // dest property will take on value of src property (automapper can't auto infer if name is different)
                .ForMember(dest => dest.JwtToken, opt => opt.Ignore()) 
            // dest property will be ignored (automapper tries to map everything, use this to ignore what shouldn't be mapped)
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            // for update function mapping, only property that isn't null will be mapped, ignored otherwise
                .ReverseMap()
            // allow the mapping to go both way, only use this if both obj are nearly/exactly identical, use business logic to handle missing properties on one end
            */
            #endregion

            CreateMap<ParkingLot, ParkingLotView>()
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.UserId));
            CreateMap<PostParkingLotForm, ParkingLot>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ManagerId));

            CreateMap<CreateFeedbackForm, Feedback>();
            CreateMap<Feedback, FeedbackView>();
            CreateMap<EditFeedbackForm, Feedback>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CarReportForm, Report>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());
            CreateMap<UserReportForm, Report>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());
            CreateMap<Report, ReportView>();
            CreateMap<EditReportForm, Report>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateInquiryForm, Inquiry>();
            CreateMap<AnswerInquiryForm, Inquiry>();
            CreateMap<Inquiry, InquiryView>();
            CreateMap<EditInquiryForm, Inquiry>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CarRegistration, CarRegView>();
            CreateMap<CarRegistration, SingleRegData>();

            CreateMap<User, UserView>();

            CreateMap<CarInfoForm, Car>();
            CreateMap<Car, CarView>()
                .ForMember(d => d.RentalRate, opt => opt.MapFrom(src => src.RentalRate));
            CreateMap<UpdateCarForm, Car>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null 
                && !(srcMember is string s && string.IsNullOrWhiteSpace(s))));

            CreateMap<DriverLicense, DriverLicenseView>()
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.LicenseName, opt => opt.MapFrom(src => src.LicenseName))
                .ForMember(dest => dest.LicenseDoB, opt => opt.MapFrom(src => src.LicenseDoB))
                .ForMember(dest => dest.LicenseClass, opt => opt.MapFrom(src => src.LicenseClass))
                .ForMember(dest => dest.LicenseIssue, opt => opt.MapFrom(src => src.LicenseIssue))
                .ForMember(dest => dest.LicenseExpiry, opt => opt.MapFrom(src => src.LicenseExpiry));
            CreateMap<DriverLicense, SingleLicenseData>();
            CreateMap<DriverLincenseInfo, DriverLicense>()
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseId))
                .ForMember(dest => dest.LicenseName, opt => opt.MapFrom(src => src.NameOnLicense))
                .ForMember(dest => dest.LicenseDoB, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.LicenseClass, opt => opt.MapFrom(src => src.Class))
                .ForMember(dest => dest.LicenseIssue, opt => opt.MapFrom(src => src.DateOfIssue))
                .ForMember(dest => dest.LicenseExpiry, opt => opt.MapFrom(src => src.DateOfExpiry));

            CreateMap<DriverLicense, DriverLicense>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateCarRentalRateForm, CarRentalRate>()
                .ForMember(d => d.OvertravelRatePerKm, opt => opt.MapFrom(src => src.OvertravelRatePerKmInDongperKM));
            CreateMap<UpdateCarRentalRateForm, CarRentalRate>()
                .ForMember(d => d.CarId, opt => opt.Ignore())
                .ForMember(d => d.OvertravelRatePerKm, opt => opt.MapFrom(src => src.OvertravelRatePerKmInDongperKM))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CarRentalRate, CarRentalRateView>();

            CreateMap<CreateScheduleForm, Schedules>();
            CreateMap<UpdateScheduleForm, Schedules>()
                .ForMember(d => d.CarId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Schedules, ScheduleView>();
            CreateMap<ScheduleImage, CICOImageView>();

            CreateMap<StaffLogAudit, StaffLogView>();
            CreateMap<CarHandoverAudit, CarHandoverView>();

            CreateMap<CarRegForm, CarRegistration>();
            CreateMap<CarRegistration, CarRegView>();
            CreateMap<UserUpdateRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt => opt.Condition(src => src.Username != null))
                .ForMember(dest => dest.Password, opt => opt.Condition(src => src.Password != null))
                //.ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => src.PhoneNumber != null))
                .ForMember(dest => dest.Fullname, opt => opt.Condition(src => src.Fullname != null))
                .ForMember(dest => dest.DateOfBirth, opt => opt.Condition(src => src.DateOfBirth != null))
                .ForMember(dest => dest.Address, opt => opt.Condition(src => src.Address != null))
                .ForMember(dest => dest.ImageAvatar, opt => opt.Condition(src => src.ImageAvatar != null))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status != null))
                .ForMember(dest => dest.Gender, opt => opt.Condition(src => src.Gender != 0));
            CreateMap<Booking, BookingView>()
                .ForMember(dest => dest.InvoiceNo, opt => opt.MapFrom(src => src.Invoice.InvoiceNo));
            CreateMap<Invoice, InvoiceView>();
            CreateMap<Invoice, InvoiceOwnerView>();
            CreateMap<InvoiceItem, InvoiceItemView>();
            CreateMap<PaymentHistory, PaymentHistoryView>();
            CreateMap<RegisterUserForm, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.GoogleId, opt => opt.MapFrom(src => src.GoogleId))
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Fullname))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.GenderId));
            CreateMap<User, UserPostRegView>();
            CreateMap<User, UserLoginView>();
            CreateMap<Schedules, ScheduleView>();
            CreateMap<PersistNotif, PersitNotifyReturn>();
            CreateMap<GPS, GPSView>();
            CreateMap<CarToll, CarTollView>();
            CreateMap<CarTollTransac, CarTollTransacView>();
            CreateMap<CarWallet, CarWalletView>();
            CreateMap<CarTravelLog, CarTravelView>();
            CreateMap<TollBooth, TollBoothView>();
        }
    }
}
