using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.CustomFunctions.TokenHandler;
using Repository.DTO;
using Repository.Extension.AutoMapper;
using Service.Services.Implementation;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<SMTPSettings>(configuration.GetSection("SMTPSettings"));
            service.AddScoped<JWTTokenProvider>();
            //service.AddAutoMapper(typeof(AutoMapperProfile));

            //service.AddScoped<IAuthenService, AuthenService>();
            service.AddScoped<IUserService, UserService>();
            service.AddScoped<IJWTService, JWTService>();
            service.AddScoped<IBookingService, BookingService>();
            service.AddScoped<IInvoiceService, InvoiceService>();
            //service.AddScoped<ICarService, CarService>();
            //service.AddScoped<IParkingLotService, ParkingLotService>();
            return service;
        }
    }
}
