using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Repository.DTO.ResponseDTO.DriverLicense;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class FPTAIService : IFPTAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        public FPTAIService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        public async Task<DriverLincenseInfo> ExtractDriverLicenseInfo(IFormFile image)
        {
            var key = _config["FPT_AI:API_KEY"];
            var url = _config["FPT_AI:Url"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", key);
            
            await using var imageStream = image.OpenReadStream();
            using var ImageContent = new StreamContent(imageStream);
            ImageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            using var multipart = new MultipartFormDataContent();
            var filename = Path.GetFileName(image.FileName) ?? "image";
            multipart.Add(ImageContent, "image", image.FileName);

            using var response = await client.PostAsync(url, multipart);
            var responseString = await response.Content.ReadAsStringAsync();
            Log.Information(responseString);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"FPT AI request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {responseString}");
            }
            var j = JObject.Parse(responseString);

            // Expecting top-level "data" array; use first element
            var dataToken = j["data"]?.FirstOrDefault();
            if (dataToken == null)
            {
                throw new Exception("FPT AI response did not contain expected 'data' element.");
            }

            static int ParseProb(JToken? token)
            {
                if (token == null) return 0;
                var s = token.ToString();
                if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                    return (int)Math.Round(d);
                return 0;
            }

            var drvLicenseInfo = new DriverLincenseInfo
            {
                LicenseId = (string?)dataToken["id"] ?? string.Empty,
                IdProbability = ParseProb(dataToken["id_prob"]),
                NameOnLicense = (string?)dataToken["name"] ?? string.Empty,
                NameProbability = ParseProb(dataToken["name_prob"]),
                Class = (string?)dataToken["class"] ?? string.Empty,
                ClassProbability = ParseProb(dataToken["class_prob"]),
                DateOfBirth = (string?)dataToken["dob"] ?? string.Empty,
                DateOfBirthProbability = ParseProb(dataToken["dob_prob"]),
                DateOfIssue = (string?)dataToken["date"] ?? string.Empty,
                DateOfIssueProbability = ParseProb(dataToken["date_prob"]),
                DateOfExpiry = (string?)dataToken["doe"] ?? string.Empty,
                DateOfExpiryProbability = ParseProb(dataToken["doe_prob"])
            };

            return drvLicenseInfo;
        }
    }
}
