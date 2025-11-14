using AutoMapper;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.ParkingLot;
using Repository.DTO.ResponseDTO.Booking;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.ParkingLot;
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

            CreateMap<CarRegistration,CarRegView>();

            CreateMap<User, UserView>();

            CreateMap<CarInfoForm, Car>();
            CreateMap<Car, CarView>();

            CreateMap<CarRegForm, CarRegistration>();
            CreateMap<CarRegistration, CarRegView>();
            CreateMap<UserUpdateRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt => opt.Condition(src => src.Username != null))
                .ForMember(dest => dest.Password, opt => opt.Condition(src => src.Password != null))
                .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => src.PhoneNumber != null))
                .ForMember(dest => dest.Fullname, opt => opt.Condition(src => src.Fullname != null))
                .ForMember(dest => dest.Address, opt => opt.Condition(src => src.Address != null))
                .ForMember(dest => dest.ImageAvatar, opt => opt.Condition(src => src.ImageAvatar != null))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status != null))
                .ForMember(dest => dest.Gender, opt => opt.Condition(src => src.Gender != 0));
            CreateMap<Booking, BookingView>()
                .ForMember(dest => dest.InvoiceNo, opt => opt.MapFrom(src => src.Invoice.InvoiceNo));
        }
    }
}
