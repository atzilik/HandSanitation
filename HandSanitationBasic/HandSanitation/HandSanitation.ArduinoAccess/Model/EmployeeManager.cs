using System;
using System.Collections.Generic;
using System.Text;
using HandSanitation.ArduinoAccess.Configurations;
using HandSanitation.Core;
using HandSanitation.Signals;

namespace HandSanitation.ArduinoAccess.Model
{
    public enum ETransitionType
    {
        RisingEdge,
        DescendEdge,
        HighLevel,
        LowLevel
    }

    public enum EDelayType
    {
        WithinTimePlus,
        WarningTimePlus,
        WithinTimeMinus,
        WarningTimeMinus,
        FailedTime
    }
    public class EmployeeManager : IEmployeeManager
    {
        private ManInTheRoom _activeManInTheRoom;
        private readonly ILedControl _ledControl;
        private readonly IParamConfig _paramConfig;
        private readonly ILogService _logService;
        private List<ManInTheRoom> ManInTheRoomList { get; set; } = new List<ManInTheRoom>();
        private int EmployeeNumber;
        private DateTime lastEntryTime;
        private DateTime lastWashTime;
        private bool inRoom = false;
        private bool isEmployee = false;
        private bool warningOn = false;
        private bool hansWaskedOK = false;
        private DateTime _lastExitTime;
        private Dictionary<ESensorType, SensorStatus> ManagerSensors { get; set; } = new Dictionary<ESensorType, SensorStatus>();

        public EmployeeManager(ILedControl ledControl, IParamConfig paramConfig, ILogService logService)
        {
            _ledControl = ledControl;
            _paramConfig = paramConfig;
            _logService = logService;
            Thresholds thresholds = _paramConfig.ReadThresholds();
            ManagerSensors.Add(ESensorType.PressureSensor, new SensorStatus(thresholds.PsLow, thresholds.PsHigh));
            ManagerSensors.Add(ESensorType.DistanceMeter1, new SensorStatus(thresholds.Dm1Low, thresholds.Dm1High));
            ManagerSensors.Add(ESensorType.DistanceMeter2, new SensorStatus(thresholds.Dm2Low, thresholds.Dm2High));
            ManagerSensors.Add(ESensorType.EmployeeCardReader, new SensorStatus(0, 0));
            _lastExitTime = lastEntryTime = lastWashTime = DateTime.Now; // init time references
          //  _logService.SerializeObjectAndSendToDb(new ManInTheRoom());
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
                           // if ((DateTime.Now - _lastExitTime).Milliseconds < 500)
                             //   break;
                            inRoom = true;

                            if (_activeManInTheRoom == null)
                            {
                                _activeManInTheRoom = new ManInTheRoom();
                            }

                            lastEntryTime = ManagerSensors[ESensorType.DistanceMeter1].ChangeTime;
                            _ledControl.ChangeColor(255, 255, 0);
                        }
                        else
                        {
                            inRoom = false;
                            if (null != _activeManInTheRoom)
                            {
                                _activeManInTheRoom.ExitTime = DateTime.Now;
                                _logService.SerializeObjectAndSendToDb(_activeManInTheRoom);// write to DB
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
                    EmployeeNumber = sensorValue;
                    if ((lastWashTime - lastEntryTime).Milliseconds < 15000)
                    {
                        hansWaskedOK = true;
                        _ledControl.ChangeColor(0, 0, 255);
                    }
                    else
                    {
                        _ledControl.ChangeColor(0, 0, 255);
                    }
                    break;
                case ESensorType.PressureSensor:
                    if (ETransitionType.RisingEdge == ManagerSensors[ESensorType.PressureSensor].GetSensorTransition(sensorValue))
                    {
                        lastWashTime = ManagerSensors[ESensorType.PressureSensor].ChangeTime;
                        // if ((lastWashTime - lastEntryTime).Milliseconds < 15000) // less than 15 sec from entry to wash - OK!!!
                        //if (inRoom)
                        //{
                            if (null != _activeManInTheRoom)
                            {
                                _activeManInTheRoom.Sanitized = true;
                                _activeManInTheRoom.SanitizeTime = lastWashTime;
                                _logService.SerializeObjectAndSendToDb(_activeManInTheRoom);
                            }
                            hansWaskedOK = true;
                            _ledControl.ChangeColor(0, 255, 0); // green
                        //}
                        //else
                        //{
                        //    hansWaskedOK = false;
                        //}
                    }
                    break;
                default:
                    break;
            }

           // var l = ((DateTime.Now - lastEntryTime)).Seconds;
            //if ((DateTime.Now - lastEntryTime).Seconds > 20)
            //{
            //    hansWaskedOK = false;
            //    warningOn = false;
            //    _ledControl.TurnToIdle();
            //    if (null != _activeManInTheRoom)
            //    {
            //        _activeManInTheRoom.ExitTime = DateTime.Now;
            //        _logService.SerializeObjectAndSendToDb(_activeManInTheRoom); //write to DB
            //        _activeManInTheRoom = null; // 
            //        _lastExitTime = DateTime.Now; // to filter noise                // turn off light after 2 minutes
            //    }
            //}
        }

        
    }


    public class SensorStatus
    {

        public SensorStatus(int thresholdsPsLow, int thresholdsPsHigh)
        {
            _sensorLowLimit = thresholdsPsLow;
            _sensorHighLimit = thresholdsPsHigh;
        }

        private int _sensorLowLimit;
        private int _sensorHighLimit;
        public DateTime ChangeTime { get; set; }
        public bool SensorHigh { get; set; }
        public ETransitionType LastTransition { get; set; } = ETransitionType.HighLevel;

        public EDelayType GetEventDelay(DateTime OthersChangeTime)
        {
            if (ETransitionType.DescendEdge != LastTransition)
                return EDelayType.FailedTime;
            int myTimeDelay = (OthersChangeTime - ChangeTime).Milliseconds;
            if (myTimeDelay > 0)
            {
                if (myTimeDelay < 15000)
                    return EDelayType.WithinTimeMinus; // after other sensor
                if (myTimeDelay < 15000)
                    return EDelayType.WarningTimeMinus; // after other sensor
                return EDelayType.FailedTime;
            }
            myTimeDelay = (ChangeTime - OthersChangeTime).Milliseconds;
            if (myTimeDelay < 15000)
                return EDelayType.WithinTimePlus; // before other sensor
            if (myTimeDelay < 15000)
                return EDelayType.WarningTimePlus; // before other sensor
            return EDelayType.FailedTime;
        }

        public ETransitionType GetSensorTransition(int newSensorVal)
        {
            if (SensorHigh)
            {
                if (newSensorVal < _sensorLowLimit) // change state
                {
                    SensorHigh = false;
                    ChangeTime = DateTime.Now;
                    LastTransition = ETransitionType.DescendEdge;
                    return LastTransition;
                }
                else
                {
                    return ETransitionType.HighLevel;
                }
            }
            else
            { // check rising Edge
                if (newSensorVal > _sensorHighLimit) // change state
                {
                    SensorHigh = true;
                    ChangeTime = DateTime.Now;
                    LastTransition = ETransitionType.RisingEdge;
                    return LastTransition;
                }
                else
                {
                    return ETransitionType.LowLevel;
                }
            }
        }
    }

}
