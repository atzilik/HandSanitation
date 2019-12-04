using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YeelightAPI;

namespace HandSanitation.Signals
{
    public class LedControl : ILedControl
    {
        private Device _device;

        public LedControl()
        {
            Connect();
            TurnToIdle();
           // _device.SetBrightness(1);

        }
        public void Connect()
        {
            _device = new Device("192.168.43.174");

            var result = _device.Connect();
        }

        public void TurnOn()
        {
            _device.SetPower(true);
        }

        public void TurnOff()
        {
            _device.SetPower(false);
        }

        public void ChangeColor(int r, int g, int b)
        {

            _device.SetRGBColor(r, g, b);
        }

        public void TurnToIdle()
        {
            _device.SetRGBColor(169, 169, 169);
        }
    }
}
