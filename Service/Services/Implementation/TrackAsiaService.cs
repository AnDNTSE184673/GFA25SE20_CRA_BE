using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class TrackAsiaService : ITrackAsiaService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        public TrackAsiaService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }
        public async Task<int?> GetDistanceBetween(string scCoord, string desCoord)
        {
            var apiKey = _config["TrackAsia:APIKEY"];
            var scCoords = await GetPlaceCoordinate(scCoord);
            var desCoords = await GetPlaceCoordinate(desCoord);
            if (scCoords == null || desCoords == null)
            {
                return null; // Unable to get coordinates for one or both addresses
            }
            var requestUrl = $"https://maps.track-asia.com/distance-matrix/v1/car/{Uri.EscapeDataString(scCoords.Value.Item1)},{Uri.EscapeDataString(scCoords.Value.Item2)};{Uri.EscapeDataString(desCoords.Value.Item1)},{Uri.EscapeDataString(desCoords.Value.Item2)}?key={apiKey}&sources=0&destinations=1&annotations=distance&fallback_speed=45";
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve data from TrackAsia API");
            }
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            if (!doc.RootElement.TryGetProperty("distances", out var distances) || distances.ValueKind != JsonValueKind.Array)
                throw new Exception("Invalid response format from TrackAsia API");            
            var firstRow = distances[0];
            if (firstRow.ValueKind != JsonValueKind.Array || firstRow.GetArrayLength() == 0)
                throw new Exception("Invalid distances format in TrackAsia API response");
            var distanceDouble = firstRow[0].GetDouble();
            return (int)Math.Round(distanceDouble);
        }

        public async Task<(string, string)?> GetPlaceCoordinate(string address)
        {
            var apiKey = _config["TrackAsia:APIKEY"];
            var requestUrl = $"https://maps.track-asia.com/api/v1/search?key={apiKey}&text={Uri.EscapeDataString(address)}&new_admin=true";
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve data from TrackAsia API");
            }
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("Invalid response format from TrackAsia API");
            }
            var firstFeature = features.EnumerateArray().FirstOrDefault();
            if (firstFeature.ValueKind == JsonValueKind.Undefined)
            {
                return null; // No results found
            }
            if (!firstFeature.TryGetProperty("geometry", out var geometry) ||
                !geometry.TryGetProperty("coordinates", out var coordinates) ||
                coordinates.ValueKind != JsonValueKind.Array ||
                coordinates.GetArrayLength() < 2)
            {
                throw new Exception("Invalid geometry format in TrackAsia API response");
            }
            var lon = coordinates[0].GetDouble();
            var lat = coordinates[1].GetDouble();
            return ($"{lon}",$"{lat}");
        }
    }
}
