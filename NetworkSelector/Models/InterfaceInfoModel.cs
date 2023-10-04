using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSelector.Models
{
    public class InterfaceInfoModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MACAddress { get; set; }
        public string IPAddress { get; set; }
        public string GatewayAddress { get; set; }
        public string DNS { get; set; }
        public string Type { get; set; }
        public string Speed { get; set; }
    }
}
