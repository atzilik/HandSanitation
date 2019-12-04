using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using HandSanitation.ArduinoAccess.Events;
using HandSanitation.ArduinoAccess.Model;
using Prism.Events;
using YeelightAPI;

namespace HandSanitation.ArduinoAccess
{
    public enum ESensorType
    {
        PressureSensor,
        DistanceMeter1,
        DistanceMeter2,
        EmployeeCardReader,
        Invalid
    }
    public class DataAccess : IDataAccess
    {
        private readonly IEmployeeManager _employeeManager;
        private readonly SubscriptionToken _eventAggregator;
        private ManInTheRoom _currentManInTheRoom;
        public string OutputLine { get; set; }

        public SerialPort SerialPort { get; set; }

        private List<ManInTheRoom> ManInTheRoomList { get; set; }

        public DataAccess(IEmployeeManager employeeManager, IEventAggregator eventAggregator)
        {
            _employeeManager = employeeManager;
            _eventAggregator = eventAggregator.GetEvent<NewEmployeeIdEvent>().Subscribe((employeeId)=>_employeeManager.NewSensorDataReceived(ESensorType.EmployeeCardReader, employeeId));
        }


        public void ReadFromPort()
        {
            try
            {
                // Initialise the serial port on COM4.
                this.SerialPort = new SerialPort("COM6", 9600);
                // Subscribe to the DataReceived event.
                this.SerialPort.DataReceived += SerialPortDataReceived;


                // Now open the port.
                this.SerialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            // Read the data that's in the serial buffer.
            var serialdata = serialPort.ReadLine();
            if (!string.IsNullOrEmpty(serialdata))
            {
                var data = serialdata.Remove(serialdata.Length - 1);
                HandleInput(data);
            }
            // Write to debug output.
            Debug.WriteLine(serialdata);
        }

        private void HandleInput(string sensorData)
        {
            try
            {
                if (!string.IsNullOrEmpty(sensorData))
                {
                    var arduinoSensorType = sensorData.Substring(0, sensorData.IndexOf(":", StringComparison.Ordinal));

                    var sensorValue =
                        int.Parse(sensorData.Substring(sensorData.IndexOf(":", StringComparison.Ordinal) + 1));

                    ESensorType sensorType = GetEnumValue(arduinoSensorType);

                    if (!sensorType.Equals(ESensorType.Invalid))
                    {
                        _employeeManager.NewSensorDataReceived(sensorType, sensorValue);
                    }
                   
                }
            }
            catch (Exception e)
            {

            }
        }

        private ESensorType GetEnumValue(string arduinoSensorType)
        {
            switch (arduinoSensorType)
            {
                case "dm1":
                    {
                        return ESensorType.DistanceMeter1;
                    }
                case "dms2":
                    {
                        return ESensorType.DistanceMeter2;
                    }
                case "ps":
                    {
                        return ESensorType.PressureSensor;
                    }
                default: return ESensorType.Invalid;
            }
        }
        private void TimeOutHandling(object sender, ElapsedEventArgs elapsedEventArgs)
        {

        }
    }
}
