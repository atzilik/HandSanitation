using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSanitation.Signals
{
    public interface ILedControl
    {
        void Connect();
        void TurnOn();
        void TurnOff();
        void ChangeColor(int r, int g, int b);
        void TurnToIdle();
    }
}
