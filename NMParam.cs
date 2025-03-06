using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMController
{
    public class NMParam
    {
        public String IP { get; set; }

        public String WiFiSSID {  get; set; }

        public String WiFiPWD { get; set; }
        public string PrimaryPool { get; set; }
        public string PrimaryPassword { get; set; }
        public string PrimaryAddress { get; set; }
        public string SecondaryPool { get; set; }
        public string SecondaryPassword { get; set; }
        public string SecondaryAddress { get; set; }
        public int Timezone { get; set; }
        public int UIRefresh { get; set; }
        public int ScreenTimeout { get; set; }
        public int Brightness { get; set; }
        public bool SaveUptime { get; set; }
        public bool LedEnable { get; set; }
        public bool RotateScreen { get; set; }
        public bool BTCPrice { get; set; }
        public bool AutoBrightness { get; set; }
    }
}
