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
        // store last known position + timestamp
        private readonly ConcurrentDictionary<Guid, (double Lat, double Lon, DateTime LastUtc)> _positions;
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

            _postUrl = config["Gps:Endpoint"] ?? "http://localhost:7184/api/gps";
            _interval = TimeSpan.FromSeconds(int.TryParse(config["Gps:IntervalSeconds"], out var s) ? s : 300);
            _positions = new ConcurrentDictionary<Guid, (double, double, DateTime)>();
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

        /// <summary>
        /// Publish one simulated GPS ping for a confirmed booking.
        /// Movement is simulated based on elapsed time and a generated speed + bearing.
        /// </summary>
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

            // Load car including preferred lot so we can get the lot address
            var car = await uow._carRepo.GetByIdWithIncludeAsync(carId, "Id", c => c.PreferredLot);
            if (car == null)
            {
                _logger.LogWarning("Car {CarId} returned null from repo", carId);
                return;
            }

            var preferredLot = car.PreferredLot; // may be null
            if (preferredLot == null)
            {
                _logger.LogDebug("Car {CarId} has no PreferredLot assigned", carId);
            }

            var preferredLotAddress = uow._lotRepo.GetById(car.PrefLotId);

            // Resolve ITrackAsiaService from the same scope (avoid injecting a scoped service into the singleton)
            var trackService = scope.ServiceProvider.GetService<ITrackAsiaService>();

            double startLat, startLon;

            if (trackService != null && !string.IsNullOrWhiteSpace(preferredLot?.Address))
            {
                try
                {
                    // GetPlaceCoordinate returns tuple of strings (longitude, latitude) per earlier note.
                    var coord = await trackService.GetPlaceCoordinate(preferredLotAddress.Address);

                    string lonStr = null!;
                    string latStr = null!;
                    try
                    {
                        // coord may be (string, string) or Nullable<(string,string)> depending on implementation
                        lonStr = coord.Value.Item1;
                        latStr = coord.Value.Item2;
                    }
                    catch
                    {
                        try
                        {
                            lonStr = coord.Value.Item1;
                            latStr = coord.Value.Item2;
                        }
                        catch
                        {
                            lonStr = null;
                            latStr = null;
                        }
                    }

                    if (!double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out startLat)
                        || !double.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, out startLon))
                    {
                        (startLat, startLon) = RandomStartPosition();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Track service failed to resolve coordinates for lot '{Address}'; falling back to random start.", preferredLot?.Address);
                    (startLat, startLon) = RandomStartPosition();
                }
            }
            else
            {
                if (preferredLot == null)
                    _logger.LogDebug("PreferredLot not loaded for car {CarId}; using random start position", carId);

                (startLat, startLon) = RandomStartPosition();
            }

            // Retrieve existing pos or initialize with start and now
            var now = DateTime.UtcNow;
            var existing = _positions.GetOrAdd(carId, (startLat, startLon, now));

            // Calculate elapsed time in seconds since last known position
            var elapsed = Math.Max(0.0, (now - existing.LastUtc).TotalSeconds);

            // Generate a speed for this ping (meters/second). Keep same value for payload and movement.
            // Speed range: 0 - 15 m/s (approx 0 - 54 km/h)
            var speedMps = Math.Round(Math.Abs(_rnd.NextDouble() * 15), 2);

            // If elapsed is very small use a tiny step to avoid jitter
            double distanceMeters = speedMps * elapsed;

            // Random bearing in radians to change direction gradually
            var bearing = (_rnd.NextDouble() * 360.0) * Math.PI / 180.0;

            // Convert distance (meters) to delta degrees
            // Approx conversions:
            const double metersPerDegLat = 111_000.0; // ~111 km per degree latitude
            var latRad = existing.Lat * Math.PI / 180.0;
            var metersPerDegLon = 111_320.0 * Math.Cos(latRad); // approximate, varies with latitude

            double deltaLatDeg = (distanceMeters * Math.Cos(bearing)) / metersPerDegLat;
            double deltaLonDeg = (distanceMeters * Math.Sin(bearing)) / (metersPerDegLon == 0 ? 111_320.0 : metersPerDegLon);

            var newLat = existing.Lat + deltaLatDeg;
            var newLon = existing.Lon + deltaLonDeg;

            // Update dictionary with new position and timestamp
            _positions[carId] = (newLat, newLon, now);

            var payload = new
            {
                carId = carId,
                latitude = Math.Round(newLat, 8),
                longitude = Math.Round(newLon, 8),
                speed = speedMps,
                timestamp = now
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
