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
    class StackTrace : IEnumerable<StackTraceLine>
    {
        private StackTraceLine[] StackTraceLines { get; }

        internal StackTrace(StackFrame[] stackFrames)
        {
            StackTraceLines = new StackTraceLine[stackFrames.Length];
            for (int i = 0; i < stackFrames.Length; i++)
            {
                StackTraceLines[i] = StackTraceLine.FromStackFrame(stackFrames[i]);
            }
        }

        internal StackTrace(string stackTrace) : this(stackTrace, StackTraceFormat.Standard) { }

        internal StackTrace(string stackTrace, StackTraceFormat format)
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
        private static Regex StackTraceLineRegex { get; } = new Regex(@"(?<method>[^()]+)(?<methodargs>\([^()]*?\))(?:\s(?:\[.*\]\s*in\s|\(at\s*\s*)(?<file>.*):(?<linenumber>\d+))?");
        private static Regex StackTraceAndroidJavaLineRegex { get; } = new Regex(@"^\s*(?<method>[a-z][^()]+)\((?<file>[^:]*)?(?::(?<linenumber>\d+))?\)");

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

        public string FrameAddress { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string IsLr { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string IsPc { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string MachoFile { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string MachoLoadAddress { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string MachoUuid { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string MachoVmAddress { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string SymbolAddress { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool InProject { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        int IStackframe.LineNumber { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
