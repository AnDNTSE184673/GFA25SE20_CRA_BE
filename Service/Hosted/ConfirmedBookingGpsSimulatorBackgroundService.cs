using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Base;
using Repository.Constant;
using Service.Services;

namespace Service.Hosted
{
    public class ConfirmedBookingGpsSimulatorBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<ConfirmedBookingGpsSimulatorBackgroundService> _logger;
        private readonly string _postUrl;
        private readonly TimeSpan _interval;
        private readonly ConcurrentDictionary<Guid, (double Lat, double Lon)> _positions;
        private readonly Random _rnd = new();

        public ConfirmedBookingGpsSimulatorBackgroundService(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            ILogger<ConfirmedBookingGpsSimulatorBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _postUrl = config["Gps:Endpoint"] ?? "http://localhost:5000/api/gps";
            _interval = TimeSpan.FromSeconds(int.TryParse(config["Gps:IntervalSeconds"], out var s) ? s : 300);
            _positions = new ConcurrentDictionary<Guid, (double, double)>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConfirmedBookingGpsSimulatorBackgroundService started. Posting to {Url} every {Interval}s", _postUrl, _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishOneGpsPingForAnyConfirmedBookingAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GPS simulator loop");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            }

            _logger.LogInformation("ConfirmedBookingGpsSimulatorBackgroundService stopping.");
        }

        private async Task PublishOneGpsPingForAnyConfirmedBookingAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var allBookings = await uow._bookingRepo.GetAllBookings();
            if (allBookings == null || allBookings.Count == 0)
            {
                _logger.LogDebug("No bookings available for simulation.");
                return;
            }

            var booking = allBookings
                .FirstOrDefault(b => string.Equals(b.Status, ConstantEnum.Statuses.CONFIRMED, StringComparison.OrdinalIgnoreCase)
                                     && b.Car != null
                                     && string.Equals(b.Car.Status, ConstantEnum.Statuses.ACTIVE, StringComparison.OrdinalIgnoreCase));

            if (booking == null)
            {
                _logger.LogDebug("No CONFIRMED booking with an ACTIVE car found.");
                return;
            }

            var carId = booking.CarId;
            var car = await uow._carRepo.GetByIdAsync(carId);

            // Resolve ITrackAsiaService from the same scope (avoid injecting a scoped service into the singleton)
            var trackService = scope.ServiceProvider.GetService<ITrackAsiaService>();

            double startLat, startLon;

            if (trackService != null && car?.PreferredLot?.Address != null)
            {
                try
                {
                    // GetPlaceCoordinate returns (string,longtitude, string, latitude) per your note.
                    var coord = await trackService.GetPlaceCoordinate(car.PreferredLot.Address);
                    // Assume coord.Item1 = longitude string, coord.Item2 = latitude string
                    var lonStr = coord.Value.Item1;
                    var latStr = coord.Value.Item2;

                    if (!double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out startLat)
                        || !double.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, out startLon))
                    {
                        (startLat, startLon) = RandomStartPosition();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Track service failed to resolve coordinates for lot; falling back to random start.");
                    (startLat, startLon) = RandomStartPosition();
                }
            }
            else
            {
                (startLat, startLon) = RandomStartPosition();
            }

            // Use carId as key, and (lat, lon) as value
            var pos = _positions.GetOrAdd(carId, (startLat, startLon));

            // small step to simulate motion
            pos = (Step(pos.Lat), Step(pos.Lon));
            _positions[carId] = pos;

            var payload = new
            {
                carId = carId,
                latitude = pos.Lat,
                longitude = pos.Lon,
                speed = Math.Round(Math.Abs(_rnd.NextDouble() * 15), 2),
                timestamp = DateTime.UtcNow
            };

            var client = _httpFactory.CreateClient();
            try
            {
                var res = await client.PostAsJsonAsync(_postUrl, payload, ct);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GPS POST returned {Status} for car {CarId}", res.StatusCode, carId);
                }
                else
                {
                    _logger.LogDebug("Posted GPS for car {CarId}: {@Payload}", carId, payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to POST simulated GPS for car {CarId}; payload logged", carId);
                _logger.LogInformation("Simulated GPS payload: {@Payload}", payload);
            }
        }

        private double Step(double coord) => coord + (_rnd.NextDouble() - 0.5) * 0.0003;

        private (double Lat, double Lon) RandomStartPosition()
        {
            var startLat = double.TryParse(Environment.GetEnvironmentVariable("GPS_START_LAT"), out var lat) ? lat : 37.773972;
            var startLon = double.TryParse(Environment.GetEnvironmentVariable("GPS_START_LON"), out var lon) ? lon : -122.431297;
            return (startLat + (_rnd.NextDouble() - 0.5) * 0.01, startLon + (_rnd.NextDouble() - 0.5) * 0.01);
        }
    }
}
