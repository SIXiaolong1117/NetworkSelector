using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSelector.Models
{
    public class NSModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Netinterface { get; set; }
        public string IPAddr { get; set; }
        public string Mask { get; set; }
        public string Gateway { get; set; }
        public string DNS1 { get; set; }
        public string DNS2 { get; set; }
    }
}
