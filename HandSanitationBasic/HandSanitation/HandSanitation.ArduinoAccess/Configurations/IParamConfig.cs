using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSanitation.ArduinoAccess.Configurations
{
    public interface IParamConfig
    {
        Thresholds ReadThresholds();
    }
}
