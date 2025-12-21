using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repository.Extension.SpeedSMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension.TextBeeDotDev
{ 
    public class TextBeeSMSAPI
    {
        private readonly IConfiguration _config;

        public TextBeeSMSAPI(IConfiguration config)
        {
            _config = config;
        }
        
        public async Task SendSMSMessage(string message, string phoneNumber)
        {
            var baseUrl = _config["TextBee:Base_URL"];
            var apiKey = _config["TextBee:API_KEY"];
            var deviceId = _config["TextBee:DeviceId"];

            var url = $"{baseUrl}/{deviceId}/send-sms";

            var requestBody = new
            {
                recipients = new[] { phoneNumber }, // MUST be array
                message = message
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"TextBee SMS failed: {response.StatusCode} - {responseBody}");
            }
        }

        public string SMSMessage(string serviceName, string otpCode)
        {
            return $"{otpCode} is your {serviceName} verification code.";
        }
    }
}