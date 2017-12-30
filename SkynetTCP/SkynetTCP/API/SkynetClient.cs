using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace SkynetTCP.API
{
    public class SkynetClient
    {
        private HttpClient _client;
        private string ZoneID { get; set; }

        public SkynetClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://skynetgdl.azurewebsites.net/")
            };
        }

        public async Task GetZoneID(string macAddress)
        {
            var response = await _client.GetAsync($"/api/config/{macAddress}");
            var json = await response.Content.ReadAsStringAsync();
            var zoneConfig = JsonConvert.DeserializeObject<MainConfig>(await response.Content.ReadAsStringAsync());
            ZoneID = zoneConfig.ZoneID.ToString();
        }

        public async Task<Configuration> GetConfig(string macAddress)
        {
            var response = await _client.GetAsync($"/api/config/{ZoneID}/{macAddress}");
            return JsonConvert.DeserializeObject<Configuration>(await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> PushUpdate(string name, string action)
        {
            var json = JsonConvert.SerializeObject(new { Action = action });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"/api/devices/{name}", content);
            return response.IsSuccessStatusCode;
        }
    }

    public class Configuration
    {
        public string ClientName { get; set; }
        public Dictionary<string, int> PinMap { get; set; } = new Dictionary<string, int>();
    }

    public class MainConfig
    {
        public Guid ZoneID { get; set; }
        public string MacAddress { get; set; }
    }
}
