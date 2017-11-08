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

        public SkynetClient(string zoneId)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://skynetgdl.azurewebsites.net/")
            };

            ZoneID = zoneId;
        }

        public async Task<Configuration> GetConfig(string macAddress)
        {
            return await Task.FromResult(new Configuration()
            {
                ClientName = "Chris Rasp",
                PinMap =
                {
                    { "LIGHT YAGAMI", 12 }
                }
            });

            /*
            var response = await _client.GetAsync($"/api/config/{ZoneID}/{macAddress}");
            return JsonConvert.DeserializeObject<Configuration>(await response.Content.ReadAsStringAsync());
            */
        }
    }

    public class Configuration
    {
        public string ClientName { get; set; }
        public Dictionary<string, int> PinMap { get; set; } = new Dictionary<string, int>();
    }
}
