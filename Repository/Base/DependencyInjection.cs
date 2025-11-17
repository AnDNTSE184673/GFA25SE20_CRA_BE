using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Repositories;
using Repository.Repositories.Interfaces;
using Serilog;

namespace Repository.Base
{
    /*
 * Dependency injection (DI) is a technique for achieving loose coupling between objects and their collaborators, or dependencies.
 * Most often, classes will declare their dependencies via their constructor, allowing them to follow the Explicit Dependencies Principle.
 * This approach is known as "constructor injection".
 * To implement dependency injection, we need to configure a DI container with classes that is participating in DI.
 * DI Container has to decide whether to return a new instance of the service or provide an existing instance.
 */

    public static class DependencyInjection
    {
        /*
         * The below three methods define the lifetime of the services:
         *  - AddTransient: Transient lifetime services are created each time they are requested.
         * This lifetime works best for lightweight, stateless services.
         *  - AddScoped: Scoped lifetime services are created once per request.
         *  - AddSingleton: Singleton lifetime services are created the first time they are requested
         * (or when ConfigureServices is run if you specify an instance there) and then every subsequent request will use the same instance.
         */

        public static IServiceCollection AddRepositories(this IServiceCollection service, IConfiguration configuration)
        {
            Log.Information("DI AddRepositories");
            //service.AddDbContext<CRA_DbContext>(options =>
            //options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            //Disable if not in use (for debugging)
            /*
            service.AddDbContext<YuuZoneDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging());
            */

            service.AddScoped<UnitOfWork>();
            service.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            //Inject dependency below

            //service.AddScoped<IAuthenRepository, AuthenRepository>();
            service.AddScoped<IUserRepository, UserRepository>();
            service.AddScoped<IBookingRepository, BookingRepository>();
            service.AddScoped<IInvoiceRepository, InvoiceRepository>();
            service.AddScoped<IParkingLotRepository, ParkingLotRepository>();
            service.AddScoped<ICarRepository, CarRepository>();
            service.AddScoped<ICarRegRepository, CarRegRepository>();
            service.AddScoped<IPaymentRepository, PaymentRepository>();
            service.AddScoped<IFeedbackRepository, FeedbackRepository>();
            service.AddScoped<IFeedbackImageRepository, FeedbackImageRepository>();
            return service;
        }
    }
}