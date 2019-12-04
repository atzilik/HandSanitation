using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Logging;

namespace HandSanitation.Core
{
    public interface ILogService : ILoggerFacade
    {
        void SetupLog();
        void SerializeObjectAndSendToDb<T>(T objectToSend);
    }
}
