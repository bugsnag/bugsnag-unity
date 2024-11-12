using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    public class LogTypeSeverityMapping
    {
        Dictionary<LogType, Severity> Mappings { get; }

        internal LogTypeSeverityMapping()
        {
            Mappings = new Dictionary<LogType, Severity> {
                { LogType.Assert, Severity.Warning },
                { LogType.Error, Severity.Warning },
                { LogType.Exception, Severity.Error },
                { LogType.Log, Severity.Info },
                { LogType.Warning, Severity.Warning },
            };
        }

        public void UpdateMapping(LogType logType, Severity severity)
        {
            Mappings[logType] = severity;
        }

        public Severity Map(LogType logType)
        {
            if (Mappings.ContainsKey(logType))
            {
                return Mappings[logType];
            }

            return Severity.Error;
        }
    }
}
