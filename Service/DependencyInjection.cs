using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.CustomFunctions.TokenHandler;
using Repository.DTO;
using Repository.Extension.AutoMapper;
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
            service.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            //service.AddScoped<IAuthenService, AuthenService>();
            return service;
        }
    }
}
