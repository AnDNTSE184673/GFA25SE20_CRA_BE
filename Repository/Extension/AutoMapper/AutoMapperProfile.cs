using AutoMapper;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
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
            /*CreateMap<User, UserLoginView>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.JwtToken, opt => opt.Ignore());
            */
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
        }
    }
}
