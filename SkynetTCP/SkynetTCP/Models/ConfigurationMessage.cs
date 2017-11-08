using System;
using System.Collections.Generic;
using System.Text;

namespace SkynetTCP.Models
{
    public class ConfigurationMessage
    {
        public Dictionary<string, int> PinMap { get; set; } = new Dictionary<string, int>();
        public string ClientName { get; set; }
    }
}
