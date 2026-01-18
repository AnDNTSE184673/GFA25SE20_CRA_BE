using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Hosted
{
    public class CarWalletCheckerBackgroundService : BackgroundService
    {
        private readonly ILogger<CarWalletCheckerBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly TimeSpan _interval;
        private readonly decimal _threshold;

        public CarWalletCheckerBackgroundService(
            ILogger<CarWalletCheckerBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = config;

            var intervalSeconds = _config.GetValue<int>("CarWalletChecker:IntervalSeconds", 3600); // default 1 hour
            _interval = TimeSpan.FromSeconds(Math.Max(1, intervalSeconds));

            _threshold = _config.GetValue<decimal>("CarWalletChecker:Threshold", 100000m); // default threshold
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CarWalletCheckerBackgroundService starting. Threshold={Threshold}, Interval={Interval}", _threshold, _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                    var wallets = await uow._carWalletRepo.GetAllAsync();
                    if (wallets == null)
                    {
                        _logger.LogDebug("No car wallets found.");
                    }
                    else
                    {
                        foreach (var wallet in wallets)
                        {
                            try
                            {
                                if (wallet.Balance < _threshold)
                                {
                                    var car = await uow._carRepo.GetByIdAsync(wallet.CarId);
                                    var userId = car?.UserId ?? Guid.Empty;
                                    if (userId == Guid.Empty)
                                    {
                                        _logger.LogWarning("Car {CarId} has no owner; skipping notification.", wallet.CarId);
                                        continue;
                                    }

                                    var license = car?.LicensePlate ?? wallet.CarId.ToString();
                                    var content = $"Low balance alert: wallet for car {license} is {wallet.Balance:N2}. Please top up to continue auto toll payments.";

                                    var notif = new PersistNotif
                                    {
                                        Id = Guid.NewGuid(),
                                        UserId = userId,
                                        Content = content,
                                        IsViewed = false,
                                        CreateDate = DateTime.UtcNow
                                    };

                                    await uow.BeginTransactionAsync();
                                    await uow._notifyRepository.CreateNotify(notif);
                                    await uow.SaveChangesAsync();
                                    await uow.CommitTransactionAsync();

                                    _logger.LogInformation("Created low-balance notification for user {UserId} (car {CarId}, balance {Balance}).", userId, wallet.CarId, wallet.Balance);
                                }
                            }
                            catch (Exception exInner)
                            {
                                _logger.LogError(exInner, "Error processing wallet {WalletId}", wallet?.Id);
                                try { await uow.RollbackTransactionAsync(); } catch { }
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown requested
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in CarWalletCheckerBackgroundService loop");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // cancellation requested - exit loop
                }
            }

            _logger.LogInformation("CarWalletCheckerBackgroundService stopping.");
        }
    }
}
