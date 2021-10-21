using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using PHIDDO.MTX.LecacyDataExtract.Models.Config;
using PHIDDO.MTX.LecacyDataExtract.Models.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PHIDDO.MTX.LecacyDataExtract.Updater.Services
{
    public class MTXApi : IMTXApi
    {
        private ILogger<MTXApi> _logger;
        private IHttpClientFactory _factory;
        private IOptionsSnapshot<ApiConfig> _apiConfig;

        public MTXApi(IOptionsSnapshot<ApiConfig> apiConfig, IHttpClientFactory factory, ILogger<MTXApi> logger)
        {
            _logger = logger;
            _factory = factory;
            _apiConfig = apiConfig;
        }

        public async Task<string> GetToken()
        {
            var token = "";
            try
            {
                HttpClient client = _factory.CreateClient();
                var payload = new
                {
                    username = _apiConfig.Value.Username,
                    password = _apiConfig.Value.Password
                };
                var payloadJson = JsonConvert.SerializeObject(payload);
                var stringContent = new StringContent(payloadJson, UnicodeEncoding.UTF8, "application/json");
                var response = await client.PostAsync(_apiConfig.Value.LoginUrl, stringContent);
                if (response != null)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var des = JsonConvert.DeserializeObject<LoginResponse>(jsonString);
                    token = des.Token;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error in Get Token: {ex.Message}");
            }
            return token;
        }

        public async Task<TravelerResult> SendCovidResult(string token, TravelerDataSet traveler)
        {
            TravelerResult result = null;
            TravelerResult tr = null;
            try
            {
                HttpClient client = _factory.CreateClient();
                var payloadJson = JsonConvert.SerializeObject(traveler);
                var serializeOptions = new JsonSerializerOptions();
                var stringContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", token);
                var response = await client.PostAsync(_apiConfig.Value.DataUrl, stringContent);
                if (response != null)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<TravelerResult>(jsonString);
                }
                if (result == null)
                {
                    tr = new TravelerResult();

                    tr.result = new List<Result>();
                    tr.result.Add(new Result()
                    {
                        error = $"Could not process record with id: {traveler.travelerData[0].public_health_case_uid}",
                        StatusCode = response.StatusCode.ToString(),
                        data = ""
                    });
                    return tr;
                }
                else
                {
                    tr = result;
                    tr.result[0].StatusCode = response.StatusCode.ToString();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error in sending covid result: {ex.Message}");
            }
            return tr;
        }
    }
}
