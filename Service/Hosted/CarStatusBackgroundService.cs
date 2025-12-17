using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Base;
using Repository.Constant;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Hosted
{
    public class CarStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CarStatusBackgroundService> _logger;
        private readonly TimeSpan _scanInterval;
        private const int DefaultScanMinutes = 5;

        public CarStatusBackgroundService(IServiceScopeFactory scopeFactory, ILogger<CarStatusBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scanInterval = TimeSpan.FromMinutes(DefaultScanMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CarStatusBackgroundService starting. Scan interval: {Interval} minutes", _scanInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ScanAndFixCarStatusesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in CarStatusBackgroundService scan loop");
                }

                try
                {
                    await Task.Delay(_scanInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // stop requested
                }
            }

            _logger.LogInformation("CarStatusBackgroundService stopping.");
        }

        private async Task ScanAndFixCarStatusesAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting car status scan...");

            // Create a scoped provider for this scan so we get a fresh UnitOfWork/DbContext
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            // Load all bookings (repository returns included relations). This is a snapshot for decision making.
            var allBookings = await unitOfWork._bookingRepo.GetAllBookings();
            if (allBookings == null || allBookings.Count == 0)
            {
                _logger.LogDebug("No bookings found during scan.");
                return;
            }

            var statusesToCheck = new[] { ConstantEnum.Statuses.CANCELLED, ConstantEnum.Statuses.COMPLETED };
            var candidateCarIds = allBookings
                .Where(b => statusesToCheck.Contains(b.Status))
                .Select(b => b.CarId)
                .Distinct()
                .ToList();

            if (!candidateCarIds.Any())
            {
                _logger.LogDebug("No candidate cars to update this run.");
                return;
            }

            try
            {
                unitOfWork.BeginTransaction();

                foreach (var carId in candidateCarIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var hasConfirmed = allBookings.Any(b => b.CarId == carId && string.Equals(b.Status, ConstantEnum.Statuses.CONFIRMED, StringComparison.OrdinalIgnoreCase));
                    if (hasConfirmed)
                    {
                        _logger.LogDebug("Car {CarId} has a CONFIRMED booking; skipping activation.", carId);
                        continue;
                    }

                    var car = await unitOfWork._carRepo.GetByIdAsync(carId);
                    if (car == null)
                    {
                        _logger.LogWarning("Car {CarId} referenced by bookings was not found.", carId);
                        continue;
                    }

                    if (!string.Equals(car.Status, ConstantEnum.Statuses.ACTIVE, StringComparison.OrdinalIgnoreCase))
                    {
                        var previous = car.Status;
                        car.Status = ConstantEnum.Statuses.ACTIVE;
                        await unitOfWork._carRepo.UpdateCarAsync(car);
                        _logger.LogInformation("Car {CarId} status changed from {Previous} to ACTIVE by background scanner.", carId, previous);
                    }
                }

                await unitOfWork.SaveChangesAsync();
                unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                _logger.LogError(ex, "Failed to update car statuses during scan; transaction rolled back.");
            }
        }
    }
}
