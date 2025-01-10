using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// The supported stacktrace parsing formats for Unity log messages
    /// </summary>
    internal enum StackTraceFormat { Standard, AndroidJava };

    /// <summary>
    /// Represents a set of Bugsnag payload stacktrace lines that are generated from a single StackTrace provided
    /// by the runtime.
    /// </summary>
    class PayloadStackTrace : IEnumerable<StackTraceLine>
    {
        public StackTraceLine[] StackTraceLines { get; private set; }

        internal PayloadStackTrace(StackFrame[] stackFrames)
        {
            StackTraceLines = new StackTraceLine[stackFrames.Length];
            for (int i = 0; i < stackFrames.Length; i++)
            {
                StackTraceLines[i] = StackTraceLine.FromStackFrame(stackFrames[i]);
            }
        }

        internal PayloadStackTrace(string stackTrace) : this(stackTrace, StackTraceFormat.Standard) { }

        internal PayloadStackTrace(string stackTrace, StackTraceFormat format)
        {
            string[] lines = stackTrace.Split(new[] {"\r\n","\r","\n", System.Environment.NewLine },
                                              System.StringSplitOptions.RemoveEmptyEntries);
            var frames = new List<StackTraceLine>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (format == StackTraceFormat.AndroidJava && i == 0)
                { // Skip the first line as it isn't a stack frame
                    continue;
                }
                var frame = format == StackTraceFormat.AndroidJava
                  ? StackTraceLine.FromAndroidJavaMessage(lines[i])
                  : StackTraceLine.FromLogMessage(lines[i]);

                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            StackTraceLines = frames.ToArray();
        }

        public IEnumerator<StackTraceLine> GetEnumerator()
        {
            foreach (var frame in StackTraceLines)
            {
                yield return frame;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Represents an individual stack trace line in the Bugsnag payload.
    /// </summary>
    public class StackTraceLine : Dictionary<string, object>, IStackframe
    {
        private static Regex StackTraceLineRegex { get; } = new Regex(@"(?:\s*at\s)?(?<method>(?:\(.*\)\s)?[^()]+)(?<methodargs>\([^()]*?\))(?:\s(?:\[.*\]\s*in\s|\(at\s*\s*)(?<file>.*):(?<linenumber>\d+))?");
        private static Regex StackTraceAndroidJavaLineRegex { get; } = new Regex(@"^\s*(?:at\s)?(?<method>[a-z][^()]+)\((?<file>[^:]*)?(?::(?<linenumber>\d+))?\)");


        public static StackTraceLine FromLogMessage(string message)
        {
            Match match = StackTraceLineRegex.Match(message);
            if (match.Success)
            {
                int? lineNumber = null;
                int parsedValue;
                if (System.Int32.TryParse(match.Groups["linenumber"].Value, out parsedValue))
                {
                    lineNumber = parsedValue;
                }
                string method = string.Join("", new string[]{match.Groups["method"].Value.Trim(),
                                                     match.Groups["methodargs"].Value});
                return new StackTraceLine(match.Groups["file"].Value,
                                          lineNumber, method);
            }
            return new StackTraceLine("", null, message);
        }

        public static StackTraceLine FromAndroidJavaMessage(string message)
        {
            Match match = StackTraceAndroidJavaLineRegex.Match(message);
            if (match.Success)
            {
                int? lineNumber = null;
                int parsedValue;
                if (System.Int32.TryParse(match.Groups["linenumber"].Value, out parsedValue))
                {
                    lineNumber = parsedValue;
                }
                string method = string.Join("", new string[] { match.Groups["method"].Value.Trim(), "()" });
                return new StackTraceLine(match.Groups["file"].Value,
                                          lineNumber, method);
            }
            // Likely a C# line in the Android stacktrace
            return FromLogMessage(message);
        }

        internal static StackTraceLine FromStackFrame(StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            var file = stackFrame.GetFileName();
            var lineNumber = stackFrame.GetFileLineNumber();
            var methodName = new Method(method).DisplayName();

            return new StackTraceLine(file, lineNumber, methodName);
        }

        internal StackTraceLine(string file, int? lineNumber, string methodName)
        {
            this.AddToPayload("file", file);
            if (lineNumber.HasValue)
            {
                this.AddToPayload("lineNumber", lineNumber.Value);
            }
            this.AddToPayload("method", methodName);
        }
        internal StackTraceLine(Dictionary<string, object> data)
        {
            foreach (var item in data)
            {
                this.AddToPayload(item.Key, item.Value);
            }
        }

        internal StackTraceLine()
        {
        }

        public string File
        {
            get
            {
                return this.Get("file") as string;
            }
            set
            {
                this.AddToPayload("file", value);
            }
        }

        public int? LineNumber
        {
            get
            {
                return this.Get("lineNumber") as int?;
            }
            set
            {
                this.AddToPayload("lineNumber", value);
            }
        }

        public string Method
        {
            get
            {
                return this.Get("method") as string;
            }
            set
            {
                this.AddToPayload("method", value);
            }
        }

        public string FrameAddress {
            get
            {
                return this.Get("frameAddress") as string;
            }
            set
            {
                this.AddToPayload("frameAddress", value);
            }
        }

        public string MachoLoadAddress {
            get
            {
                return this.Get("machoLoadAddress") as string;
            }
            set
            {
                this.AddToPayload("machoLoadAddress", value);
            }
        }

        public string MachoFile {
            get
            {
                return this.Get("machoFile") as string;
            }
            set
            {
                this.AddToPayload("machoFile", value);
            }
        }

        public string MachoUuid {
            get
            {
                return this.Get("machoUUID") as string;
            }
            set
            {
                this.AddToPayload("machoUUID", value);
            }
        }

        public bool? InProject
        {
            get
            {
                return this.Get("inProject") as bool?;
            }
            set
            {
                this.AddToPayload("inProject", value);
            }
        }


        public bool? IsLr { get; set; }
        public bool? IsPc { get; set; }
        public string MachoVmAddress { get; set; }
        public string SymbolAddress { get; set; }
    }
}
