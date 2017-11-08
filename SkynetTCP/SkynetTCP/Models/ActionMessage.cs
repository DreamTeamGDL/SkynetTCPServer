using System;
using System.Collections.Generic;
using System.Text;

namespace SkynetTCP.Models
{
    public class ActionMessage
    {
        public ACTIONS Action { get; set; }
        public string Name { get; set; }
        public string Do { get; set; }
    }
}
