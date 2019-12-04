using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace HandSanitation.ArduinoAccess.Model
{
    public class ManInTheRoom
    {
        public ManInTheRoom()
        {
            RoomEnterTimeStamp = DateTime.Now;    
        }

        public DateTime RoomEnterTimeStamp { get; set; }
        public DateTime SanitizeTime { get; set; }
        public DateTime ExitTime { get; set; }
        public int Id { get; set; } = -1;
        public bool IsEmployee { get; set; }
        public bool Sanitized { get; set; }



    }
}
