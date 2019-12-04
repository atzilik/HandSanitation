using System;
using System.Collections.Generic;
using System.Text;

namespace HandSanitation.ArduinoAccess.Model
{
    public interface IEmployeeManager
    {
        void NewSensorDataReceived(ESensorType sensorType, int sensorValue);

    }
}
