using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSanitation.ArduinoAccess.Configurations
{
    public class Thresholds
    {
        public int Dm1Low { get; set; } = 150;
        public int Dm1High { get; set; } = 450;
        public int Dm2Low { get; set; } = 150;
        public int Dm2High { get; set; } = 450;
        public int PsLow { get; set; } = 10;
        public int PsHigh { get; set; } = 100;

    }
}
