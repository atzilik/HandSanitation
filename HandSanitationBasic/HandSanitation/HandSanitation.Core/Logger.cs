using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HandSanitation.Core.Events;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Logging;

namespace HandSanitation.Core
{
    public class Logger : ILogService
    {
        private readonly IEventAggregator _eventAggregator;


        public Logger(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

        }
        private readonly ILog _logger = LogManager.GetLogger(typeof(Logger));

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    _logger.Debug($"{message}");
                    break;
                case Category.Warn:
                    _logger.Warn($"{message}");
                    break;
                case Category.Exception:
                    _logger.Error($"{message}");
                    break;
                case Category.Info:
                    _logger.Info($"{message}");
                    break;
            }
        }

        public void SetupLog()
        {
           // XmlConfigurator.Configure(new FileInfo("Configuration.xml"));
        }

        public void SerializeObjectAndSendToDb<T>(T objectToSend)
        {
           string jsonString = JsonConvert.SerializeObject(objectToSend);
           Log(jsonString, Category.Info, Priority.None);
           _eventAggregator.GetEvent<AddedToDbEvent>().Publish();
           Debug.WriteLine(jsonString);
        }
    }
}
