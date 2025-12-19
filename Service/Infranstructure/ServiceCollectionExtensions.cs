using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Infranstructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGpsStore(this IServiceCollection services)
        {
            services.AddSingleton<IGpsStore, GpsStore>();
            return services;
        }
    }
}
