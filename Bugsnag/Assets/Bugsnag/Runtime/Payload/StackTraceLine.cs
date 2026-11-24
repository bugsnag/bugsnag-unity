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
            string[] lines = stackTrace.Split(new[] { "\r\n", "\r", "\n", System.Environment.NewLine },
                                              System.StringSplitOptions.RemoveEmptyEntries);
            var frames = new List<StackTraceLine>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (format == StackTraceFormat.AndroidJava && i == 0)
                { // Skip the first line as it isn't a stack frame
                    continue;
                }
                // Skip rethrow markers that appear in exception stack traces
                // These lines don't have corresponding native addresses in IL2CPP builds
                if (lines[i].TrimStart().StartsWith("---"))
                {
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
        private static Regex StackTraceAndroidCSLineRegex { get; } = new Regex(@"^\s*(?:at\s)?(?<method>[a-zA-Z][^()]+)\s?(?<methodargs>\([^()]*?\))\s?\(at\s(?<file>[^:]*)?(?::(?<linenumber>\d+))\)");
        private static Regex StackTraceAndroidJavaLineRegex { get; } = new Regex(@"^\s*(?:at\s)?(?<method>[a-z][^()]+)\((?<file>[^:]*)?(?::(?<linenumber>\d+))?\)");


        private static StackTraceLine FromMatchedLogMessage(Match match)
        {
            int? lineNumber = null;
            int parsedValue;
            if (System.Int32.TryParse(match.Groups["linenumber"].Value, out parsedValue))
            {
                lineNumber = parsedValue;
            }
            string method = string.Join("", new string[]{match.Groups["method"].Value.Trim(),
                                                 match.Groups["methodargs"].Value});
            return new StackTraceLine(match.Groups["file"].Value, lineNumber, method);
        }

        public static StackTraceLine FromLogMessage(string message)
        {
            Match match = StackTraceLineRegex.Match(message);
            if (match.Success)
            {
                return FromMatchedLogMessage(match);
            }
            return new StackTraceLine("", null, message);
        }

        public static StackTraceLine FromAndroidJavaMessage(string message)
        {
            // Hitting AndroidJNISafe.CheckException switches the stack frame format from Android -> Standard
            // as such we always check if the log message is a Standard match first
            Match match = StackTraceAndroidCSLineRegex.Match(message);
            if (!match.Success)
            {
                match = StackTraceAndroidJavaLineRegex.Match(message);
            }

            if (match.Success)
            {
                int? lineNumber = null;
                int parsedValue;
                if (System.Int32.TryParse(match.Groups["linenumber"].Value, out parsedValue))
                {
                    lineNumber = parsedValue;
                }
                Group argsGroup = match.Groups["methodargs"];
                string args = argsGroup.Success ? argsGroup.Value : "()";

                string method = string.Join("", new string[] { match.Groups["method"].Value.Trim(), args });
                return new StackTraceLine(match.Groups["file"].Value, lineNumber, method);
            }
            // Likely a C# line in the Android stacktrace
            return FromLogMessage(message);
        }

        public static StackTraceLine FromStackFrame(StackFrame stackFrame)
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

        public string FrameAddress
        {
            get
            {
                return this.Get("frameAddress") as string;
            }
            set
            {
                this.AddToPayload("frameAddress", value);
            }
        }

        public string LoadAddress
        {
            get
            {
                return this.Get("loadAddress") as string;
            }
            set
            {
                this.AddToPayload("loadAddress", value);
            }
        }

        public string MachoLoadAddress
        {
            get
            {
                return this.Get("machoLoadAddress") as string;
            }
            set
            {
                this.AddToPayload("machoLoadAddress", value);
            }
        }

        public string MachoFile
        {
            get
            {
                return this.Get("machoFile") as string;
            }
            set
            {
                this.AddToPayload("machoFile", value);
            }
        }

        public string MachoUuid
        {
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

        public string CodeIdentifier
        {
            get
            {
                return this.Get("codeIdentifier") as string;
            }
            set
            {
                this.AddToPayload("codeIdentifier", value);
            }
        }

        public string Type
        {
            get
            {
                return this.Get("type") as string;
            }
            set
            {
                this.AddToPayload("type", value);
            }
        }


        public bool? IsLr { get; set; }
        public bool? IsPc
        {
            get
            {
                return this.Get("isPC") as bool?;
            }
            set
            {
                this.AddToPayload("isPC", value);
            }
        }

        public string MachoVmAddress { get; set; }
        public string SymbolAddress { get; set; }
    }

    /// <summary>
    /// Decides whether managed (C#) stack frames belong to the app or to engine/third-party code.
    /// Works on index ranges of the existing strings so it allocates nothing per frame.
    /// </summary>
    internal static class InProjectResolver
    {
        internal static void ApplyToErrors(IEnumerable<Error> errors, Configuration config)
        {
            if (errors == null || config == null)
            {
                return;
            }
            var include = config.ProjectNamespaces;
            var exclude = config.ExcludedNamespaces;
            var includeMode = HasAnyPrefix(include);
            foreach (var error in errors)
            {
                // Java/Kotlin frames are handled by the Android SDK/server using ProjectPackages
                if (error == null || error.IsAndroidJavaException || error.Stacktrace == null)
                {
                    continue;
                }
                foreach (var frame in error.Stacktrace)
                {
                    Apply(frame, include, exclude, includeMode);
                }
            }
        }

        internal static void Apply(IStackframe frame, string[] include, string[] exclude)
        {
            Apply(frame, include, exclude, HasAnyPrefix(include));
        }

        // A frame already marked as not in-project (e.g. an iOS frame from a non-main image) is never changed
        private static void Apply(IStackframe frame, string[] include, string[] exclude, bool includeMode)
        {
            if (frame == null || frame.InProject == false)
            {
                return;
            }
            frame.InProject = Resolve(frame.Method, include, exclude, includeMode);
        }

        internal static bool IsInProject(string method, string[] include, string[] exclude)
        {
            return Resolve(method, include, exclude, HasAnyPrefix(include));
        }

        /// <summary>
        /// The longest matching prefix wins. On a tie the project namespace wins. When nothing matches,
        /// the frame is in-project unless project namespaces were configured.
        /// </summary>
        private static bool Resolve(string method, string[] include, string[] exclude, bool includeMode)
        {
            int start, end;
            GetNameBounds(method, out start, out end);
            if (end > start)
            {
                var includeLength = LongestMatch(method, start, end, include);
                var excludeLength = LongestMatch(method, start, end, exclude);
                if (includeLength > 0 || excludeLength > 0)
                {
                    return includeLength >= excludeLength;
                }
            }
            return !includeMode;
        }

        // Finds the qualified name inside e.g. "(wrapper managed-to-native) UnityEngine.Debug:Log (Object)"
        private static void GetNameBounds(string method, out int start, out int end)
        {
            start = 0;
            end = 0;
            if (string.IsNullOrEmpty(method))
            {
                return;
            }
            var length = method.Length;
            while (start < length && char.IsWhiteSpace(method[start])) start++;
            if (start < length && method[start] == '(')
            {
                var close = method.IndexOf(')', start);
                if (close < 0 || close + 1 >= length || !char.IsWhiteSpace(method[close + 1]))
                {
                    return;
                }
                start = close + 1;
                while (start < length && char.IsWhiteSpace(method[start])) start++;
            }
            var args = method.IndexOf('(', start);
            end = args < 0 ? length : args;
            while (end > start && char.IsWhiteSpace(method[end - 1])) end--;
        }

        private static bool HasAnyPrefix(string[] prefixes)
        {
            if (prefixes == null)
            {
                return false;
            }
            foreach (var prefix in prefixes)
            {
                int ps, pe;
                if (GetPrefixBounds(prefix, out ps, out pe))
                {
                    return true;
                }
            }
            return false;
        }

        // Trims whitespace and trailing dots without allocating
        private static bool GetPrefixBounds(string prefix, out int start, out int end)
        {
            start = 0;
            end = prefix == null ? 0 : prefix.Length;
            while (start < end && char.IsWhiteSpace(prefix[start])) start++;
            while (end > start && (char.IsWhiteSpace(prefix[end - 1]) || prefix[end - 1] == '.')) end--;
            return end > start;
        }

        // "System" matches "System.IO.File.Open" but not "SystemShock.Player.Die"
        private static int LongestMatch(string name, int start, int end, string[] prefixes)
        {
            if (prefixes == null)
            {
                return 0;
            }
            var longest = 0;
            foreach (var prefix in prefixes)
            {
                int ps, pe;
                if (!GetPrefixBounds(prefix, out ps, out pe))
                {
                    continue;
                }
                var length = pe - ps;
                if (length <= longest || length > end - start
                    || string.CompareOrdinal(name, start, prefix, ps, length) != 0)
                {
                    continue;
                }
                if (start + length == end || IsBoundary(name[start + length]))
                {
                    longest = length;
                }
            }
            return longest;
        }

        private static bool IsBoundary(char c)
        {
            return c == '.' || c == ':' || c == '+' || c == '/' || c == '`' || c == '<';
        }
    }
}
