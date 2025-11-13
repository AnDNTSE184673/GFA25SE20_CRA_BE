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
using Services.Service;
using Services.Service.Implementation;
using Supabase;
using Repository.CustomFunctions.SupabaseFileUploader;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection service, IConfiguration configuration)
        {
            Log.Information("DI AddServices");
            service.Configure<SMTPSettings>(configuration.GetSection("SMTPSettings"));
            service.AddScoped<JWTTokenProvider>();
            /*service.AddAutoMapper(config =>
            {
                config.AddProfile<AutoMapperProfile>();
            });*/ //for automapper 15.0+
            service.AddAutoMapper(typeof(AutoMapperProfile));
            service.AddSingleton<Supabase.Client>(o =>
            {
                var url = configuration["Supabase:Url"];
                var key = configuration["Supabase:PrivateKey"];
                //var key = configuration["Supabase:PublicKey"];


                Log.Information("Supabase client configuring with URL: {Url}", url);
                Log.Information("Supabase key prefix: {KeyPrefix}", key?.Substring(Math.Max(0, key.Length - 10)));

                return new Supabase.Client
                (
                    url,
                    key,
                    new SupabaseOptions
                    {
                        AutoRefreshToken = true,
                        AutoConnectRealtime = true,
                    }
                );
            });
            service.AddScoped<UploadFile>();
            //service.AddAutoMapper(typeof(AutoMapperProfile));

            //service.AddScoped<IAuthenService, AuthenService>();
            service.AddScoped<IUserService, UserService>();
            service.AddScoped<IJWTService, JWTService>();
            service.AddScoped<ICarService, CarService>();
            service.AddScoped<ICarRegService, CarRegService>();
            service.AddScoped<IParkingLotService, ParkingLotService>();
            service.AddScoped<IEmailService, EmailService>();
            return service;
        }
    }
}
