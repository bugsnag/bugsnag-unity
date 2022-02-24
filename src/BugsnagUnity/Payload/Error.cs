using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents a set of Bugsnag payload exceptions that are generated from a single exception by resolving
    /// the inner exceptions present.
    /// </summary>
    class Errors : IEnumerable<Error>
    {
        private IEnumerable<Error> UnwoundExceptions { get; }

        internal Errors(System.Exception exception, System.Diagnostics.StackFrame[] alternativeStackTrace)
        {
            UnwoundExceptions = FlattenAndReverseExceptionTree(exception).Select(e => Error.FromSystemException(e, alternativeStackTrace));
        }

        internal Errors(System.Exception exception, string providedStackTrace)
        {
            UnwoundExceptions = FlattenAndReverseExceptionTree(exception).Select(e => Error.FromSystemException(e, providedStackTrace));
        }

        public IEnumerator<Error> GetEnumerator()
        {
            return UnwoundExceptions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static IEnumerable<System.Exception> FlattenAndReverseExceptionTree(System.Exception ex)
        {
            if (ex == null) yield break;

            yield return ex;

            switch (ex)
            {
                case ReflectionTypeLoadException typeLoadException:
                    foreach (var exception in typeLoadException.LoaderExceptions)
                    {
                        foreach (var item in FlattenAndReverseExceptionTree(exception))
                        {
                            yield return item;
                        }
                    }
                    break;
                default:
                    foreach (var item in FlattenAndReverseExceptionTree(ex.InnerException))
                    {
                        yield return item;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Represents an individual error in the Bugsnag payload.
    /// </summary>
    public class Error : Dictionary<string, object>, IError
    {
        internal HandledState HandledState { get; }

        internal bool IsAndroidJavaException;

        private const string ANDROID_JAVA_EXCEPTION_CLASS = "AndroidJavaException";
        private const string ERROR_CLASS_MESSAGE_PATTERN = @"^(?<errorClass>\S+):\s+(?<message>.*)";
        private const string NATIVE_ANDROID_ERROR_CLASS = "java.lang.Error";
        private const string NATIVE_ANDROID_MESSAGE_PATTERN = @"signal \d+ \(SIG\w+\)";

        private const string ERROR_CLASS_KEY = "errorClass";
        private const string MESSAGE_KEY = "message";
        private const string STACKTRACE_KEY = "stacktrace";

        internal Error(Dictionary<string, object> data)
        {
            foreach (var item in data)
            {
                this.AddToPayload(item.Key, item.Value);
            }
        }

        internal Error(string errorClass, string message, StackTraceLine[] stackTrace)
          : this(errorClass, message, stackTrace, HandledState.ForHandledException(),false) { }

        internal Error(string errorClass, string message, IStackframe[] stackTrace, HandledState handledState,bool isAndroidJavaException)
        {
            ErrorClass = errorClass;
            ErrorMessage = message;
            _stacktrace = stackTrace.ToList();
            HandledState = handledState;
            IsAndroidJavaException = isAndroidJavaException;
        }


        private List<IStackframe> _stacktrace { get => this.Get(STACKTRACE_KEY) as List<IStackframe>; set => this.AddToPayload(STACKTRACE_KEY, value); }

        public List<IStackframe> Stacktrace => _stacktrace;

        public string ErrorClass
        {
            get => this.Get(ERROR_CLASS_KEY) as string;
            set => this.AddToPayload(ERROR_CLASS_KEY, value);
        }

        public string ErrorMessage
        {
            get => this.Get(MESSAGE_KEY) as string;
            set => this.AddToPayload(MESSAGE_KEY, value);
        }

        public string Type { get => "Unity"; }

        internal static Error FromSystemException(System.Exception exception, System.Diagnostics.StackFrame[] alternativeStackTrace)
        {
            var errorClass = exception.GetType().Name;
            var stackFrames = new System.Diagnostics.StackTrace(exception, true).GetFrames();

            StackTraceLine[] lines = null;

            if (stackFrames != null && stackFrames.Length > 0)
            {
                lines = new StackTrace(stackFrames).ToArray();
            }
            else
            {
                lines = new StackTrace(alternativeStackTrace).ToArray();
            }

            return new Error(errorClass, exception.Message, lines);
        }

        internal static Error FromStringInfo(string name, string message, string stacktrace)
        {
            var stackFrames = new StackTrace(stacktrace).ToArray();
            return new Error(name, message, stackFrames);
        }

        internal static Error FromSystemException(System.Exception exception, string stackTrace)
        {
            var errorClass = exception.GetType().Name;
            var lines = new StackTrace(stackTrace).ToArray();            

            return new Error(errorClass, exception.Message, lines);
        }

        public static Error FromUnityLogMessage(UnityLogMessage logMessage, System.Diagnostics.StackFrame[] stackFrames, Severity severity)
        {
            return FromUnityLogMessage(logMessage, stackFrames, severity, false);
        }

        public static Error FromUnityLogMessage(UnityLogMessage logMessage, System.Diagnostics.StackFrame[] fallbackStackFrames, Severity severity, bool forceUnhandled)
        {
            var match = Regex.Match(logMessage.Condition, ERROR_CLASS_MESSAGE_PATTERN, RegexOptions.Singleline);

            var lines = new StackTrace(logMessage.StackTrace).ToArray();
            if (lines.Length == 0)
            {
                lines = new StackTrace(fallbackStackFrames).ToArray();
            }

            var handledState = forceUnhandled
              ? HandledState.ForUnhandledException()
              : HandledState.ForUnityLogMessage(severity);

            if (match.Success)
            {
                var errorClass = match.Groups["errorClass"].Value;
                var message = match.Groups["message"].Value.Trim();
                var isAndroidJavaException = false;
                // Exceptions starting with "AndroidJavaException" are uncaught Java exceptions reported
                // via the Unity log handler
                if (errorClass == ANDROID_JAVA_EXCEPTION_CLASS)
                {
                    isAndroidJavaException = true;
                    match = Regex.Match(message, ERROR_CLASS_MESSAGE_PATTERN, RegexOptions.Singleline);

                    // If the message matches the "class: message" pattern, then the Java class is followed
                    // by a description of the Java exception. These two values will be used as the error
                    // class and message.
                    if (match.Success)
                    {
                        errorClass = match.Groups["errorClass"].Value;
                        message = match.Groups["message"].Value.Trim();
                    }
                    else
                    {
                        // There was no Java exception description, so the Java class is the only content in
                        // the message.
                        errorClass = message;
                        message = "";
                    }
                    lines = new StackTrace(logMessage.StackTrace, StackTraceFormat.AndroidJava).ToArray();
                    handledState = HandledState.ForUnhandledException();
                }
                return new Error(errorClass, message, lines, handledState, isAndroidJavaException);
            }
            else
            {
                // include the type somehow in there
                return new Error($"UnityLog{logMessage.Type}", logMessage.Condition, lines, handledState, false);
            }
        }

        /// <summary>
        /// Validates the logMessage excluding previously delivered reports
        /// </summary>
        public static bool ShouldSend(UnityLogMessage logMessage)
        {
            if (logMessage.StackTrace == null)
            {
                return true;
            }
            // Discard any messages matching native Android events as they are captured (and more
            // accurate) via bugsnag-android. Any Java `Error` generated from a POSIX signal is
            // discarded. For Unity 2017/2018:
            var match = Regex.Match(logMessage.Condition, ERROR_CLASS_MESSAGE_PATTERN, RegexOptions.Singleline);
            if (!match.Success)
            {
                return true;
            }

            var errorClass = match.Groups["errorClass"].Value;
            if (errorClass != ANDROID_JAVA_EXCEPTION_CLASS)
            {
                return true;
            }

            var message = match.Groups["message"].Value;
            match = Regex.Match(message, ERROR_CLASS_MESSAGE_PATTERN, RegexOptions.Singleline);
            errorClass = match.Success ? match.Groups["errorClass"].Value : message;
            if (errorClass != NATIVE_ANDROID_ERROR_CLASS)
            {
                return true;
            }

            match = Regex.Match(logMessage.StackTrace, NATIVE_ANDROID_MESSAGE_PATTERN, RegexOptions.Singleline);
            return !match.Success;
        }
    }
}
