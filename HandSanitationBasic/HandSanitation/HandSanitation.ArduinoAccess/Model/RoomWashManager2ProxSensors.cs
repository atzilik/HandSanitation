using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandSanitation.ArduinoAccess.Configurations;
using HandSanitation.Signals;

namespace HandSanitation.ArduinoAccess.Model
{
    // RoomWashManager2ProxSensors
    // one sensor before the washer, one after the washer
    // first sensor: if (!inRoom) yellow light ON
    // card: add ID to the class; blue light ON
    // Wash: green light ON
    // sensor2: if (inRoom) yellow light+inRoom=false+add to DB, else inRoom = true
    // timer: 60 sec till off
    public class RoomWashManager2ProxSensors : IEmployeeManager
    {
        private List<ManInTheRoom> ManInTheRoomList { get; set; } = new List<ManInTheRoom>();
        private ManInTheRoom _activeManInTheRoom;
        private Int32 EmployeeNumber;
        private DateTime lastEntryTime;
        private DateTime _lastExitTime;
        private DateTime lastWashTime;
        //private Int32 lastEntryTime;
        //private Int32 lastWashTime;
        private bool inRoom = false;
        private bool isEmployee = false;
        private bool warningOn = false;
        private bool hansWaskedOK = false;
        private ILedControl _ledControl;
        private IParamConfig _paramConfig;

        private Dictionary<ESensorType, SensorStatus> ManagerSensors { get; set; } = new Dictionary<ESensorType, SensorStatus>();


        public RoomWashManager2ProxSensors(ILedControl ledControl, IParamConfig paramConfig)
        {
            _ledControl = ledControl;
            _paramConfig = paramConfig;
            _ledControl.Connect();
            _ledControl.TurnOn();
            _ledControl.ChangeColor(69, 69, 69);
            Thresholds thresholds = _paramConfig.ReadThresholds();
            ManagerSensors.Add(ESensorType.PressureSensor, new SensorStatus(thresholds.PsLow, thresholds.PsHigh));
            ManagerSensors.Add(ESensorType.DistanceMeter1, new SensorStatus(thresholds.Dm1Low, thresholds.Dm1High));
            ManagerSensors.Add(ESensorType.DistanceMeter2, new SensorStatus(thresholds.Dm2Low, thresholds.Dm2High));
            ManagerSensors.Add(ESensorType.EmployeeCardReader, new SensorStatus(0, 0));
            _lastExitTime = lastEntryTime = lastWashTime = DateTime.Now; // init time references
        }
        public void NewSensorDataReceived(ESensorType sensorType, int sensorValue)
        {
            switch (sensorType)
            {
                case ESensorType.DistanceMeter1:
                    if (ETransitionType.RisingEdge == ManagerSensors[ESensorType.DistanceMeter1].GetSensorTransition(sensorValue))
                    {
                        if (!inRoom)
                        {
                            if ((DateTime.Now - _lastExitTime).Milliseconds < 500)
                                break;  // filter noise
                            inRoom = true;
                            if (null == _activeManInTheRoom)
                            {
                                _activeManInTheRoom = new ManInTheRoom();
                            }
                            lastEntryTime = ManagerSensors[ESensorType.DistanceMeter1].ChangeTime;
                            _ledControl.ChangeColor(255, 255, 0);   // yellow
                        }
                        else
                        {
                            inRoom = false;
                            if (null != _activeManInTheRoom)
                            {
                                _activeManInTheRoom.ExitTime = DateTime.Now;
                                // write to DB
                                _activeManInTheRoom = null;     // 
                                _lastExitTime = DateTime.Now;    // to filter noise
                            }
                        }
                    }
                    break;
                case ESensorType.EmployeeCardReader:
                    isEmployee = true;
                    if (null == _activeManInTheRoom)
                    {
                        _activeManInTheRoom = new ManInTheRoom();
                    }
                    _activeManInTheRoom.Id = sensorValue;
                    _activeManInTheRoom.IsEmployee = true;
                    //EmployeeNumber = sensorValue;
                    if (Math.Abs((lastWashTime - lastEntryTime).Milliseconds) < 15000)
                    {
                        hansWaskedOK = true;
                        _ledControl.ChangeColor(0, 0, 255); // green
                    }
                    else
                    {
                        _ledControl.ChangeColor(0, 0, 255); // blue
                    }
                    break;
                case ESensorType.PressureSensor:
                    if (ETransitionType.RisingEdge == ManagerSensors[ESensorType.PressureSensor].GetSensorTransition(sensorValue))
                    {
                        lastWashTime = ManagerSensors[ESensorType.PressureSensor].ChangeTime;
                      //if ((lastWashTime - lastEntryTime).Milliseconds < 15000) // less than 15 sec from entry to wash - OK!!!
                        if (inRoom)
                        {
                            if (null != _activeManInTheRoom)
                            {
                                _activeManInTheRoom.Sanitized = true;
                                _activeManInTheRoom.SanitizeTime = lastWashTime;
                            }

                            hansWaskedOK = true;
                            _ledControl.ChangeColor(0, 0, 255); // green
                        }
                    }
                    break;
                case ESensorType.DistanceMeter2:
                    if (ETransitionType.RisingEdge == ManagerSensors[ESensorType.DistanceMeter2].GetSensorTransition(sensorValue))
                    {
                        if (inRoom)
                        {
                            if ((DateTime.Now - lastEntryTime).Milliseconds < 1000)
                                break;  // filter noise
                            _ledControl.ChangeColor(255, 255, 0);   // yellow
                        }
                    }
                    break;
                default:
                    break;
            }
            // periodic checks
            if (inRoom)
            {
                if ((!warningOn) && (!hansWaskedOK))
                {
                    if ((DateTime.Now - lastEntryTime).Milliseconds > 15000)
                    {
                        warningOn = true;
                        // turn on yellow light
                    }

                }

            }
            if ((DateTime.Now - lastEntryTime).Milliseconds > 60000)
            {
                hansWaskedOK = false;
                warningOn = false;
                // turn off light after 2 minutes
            }
        }


    }
}
