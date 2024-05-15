using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Events;
#nullable enable

namespace BugsnagUnity.Payload
{
    public class Correlation : PayloadContainer
    {
       
        private const string TRACE_ID_KEY = "traceid";
        private const string SPAN_ID_KEY = "string";

        internal Correlation(string traceId, string spanId)
        {
            TraceId = traceId;
            SpanId = spanId;  
        }

        public string TraceId
        {
            get {
                var @object = Get(TRACE_ID_KEY);
                if (@object == null)
                {
                    return string.Empty;
                }
                return (string)@object;
            } 
            set
            {
                Add(TRACE_ID_KEY, value);
            }  
        }

        public string SpanId
        {
            get
            {
                var @object = Get(SPAN_ID_KEY);
                if (@object == null)
                {
                    return string.Empty;
                }
                return (string)@object;
            }
            set
            {
                Add(SPAN_ID_KEY, value);
            }
        }
    }
}
