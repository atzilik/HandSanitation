using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HandSanitation.ArduinoAccess.Configurations
{
    public class ParamConfig : IParamConfig
    {

        public ParamConfig()
        {
            
        }

        public Thresholds ReadThresholds()
        {
            try
            {
                string s = File.ReadAllText("Configurations\\Thresholds.json");
                Thresholds thresholds = JsonConvert.DeserializeObject<Thresholds>(s);
                return thresholds;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while reading JSON");
                return null;
            }
        
        }
    }
}
