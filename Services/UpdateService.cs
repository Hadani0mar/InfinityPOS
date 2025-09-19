using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartInventoryPro.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://102.213.180.199:8080/infinitypos-api/";

        public UpdateService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{API_BASE_URL}check-updates.php");
                return JsonConvert.DeserializeObject<UpdateInfo>(response);
            }
            catch (Exception ex)
            {
                return new UpdateInfo
                {
                    HasUpdates = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<UpdateResult> ApplyUpdateAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{API_BASE_URL}apply-update.php");
                return JsonConvert.DeserializeObject<UpdateResult>(response);
            }
            catch (Exception ex)
            {
                return new UpdateResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }

    public class UpdateInfo
    {
        public bool HasUpdates { get; set; }
        public string LocalCommit { get; set; }
        public string RemoteCommit { get; set; }
        public string LastMessage { get; set; }
        public string LastHash { get; set; }
        public string LastDate { get; set; }
        public string Timestamp { get; set; }
        public string Error { get; set; }
    }

    public class UpdateResult
    {
        public bool Success { get; set; }
        public string NewCommit { get; set; }
        public string NewMessage { get; set; }
        public string NewDate { get; set; }
        public string Error { get; set; }
    }
}
