using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents an individual error in the Bugsnag payload.
    /// </summary>
    public class Error : Dictionary<string, object>, IError
    {
        internal HandledState HandledState { get; }

        internal bool IsAndroidJavaException;

#if ENABLE_IL2CPP
        const string COMPILATION_PLATFORM = "il2cpp"; 
#elif ENABLE_MONO
        const string COMPILATION_PLATFORM = "mono"; 
#else
        const string COMPILATION_PLATFORM = "unknown"; 
#endif

        private const string ANDROID_JAVA_EXCEPTION_CLASS = "AndroidJavaException";
        private const string ERROR_CLASS_MESSAGE_PATTERN = @"^(?<errorClass>\S+):\s+(?<message>.*)";
        private const string NATIVE_ANDROID_ERROR_CLASS = "java.lang.Error";
        private const string NATIVE_ANDROID_MESSAGE_PATTERN = @"signal \d+ \(SIG\w+\)";

        private const string ERROR_CLASS_KEY = "errorClass";
        private const string MESSAGE_KEY = "message";
        private const string STACKTRACE_KEY = "stacktrace";
        private const string ERROR_TYPE_KEY = "type";

        internal Error(Dictionary<string, object> data)
        {
            foreach (var item in data)
            {
                if (item.Key == STACKTRACE_KEY)
                {
                    var stackArray = (JsonArray)data[STACKTRACE_KEY];
                    var stackList = new List<IStackframe>();
                    foreach (JsonObject jsonFrame in stackArray)
                    {
                        var newFrame = new StackTraceLine(jsonFrame.GetDictionary());
                        stackList.Add(newFrame);
                    }
                    _stacktrace = stackList;
                }
                else
                {
                    this.AddToPayload(item.Key, item.Value);
                }
            }
        }

        internal Error(string errorClass, string message, StackTraceLine[] stackTrace)
          : this(errorClass, message, stackTrace, HandledState.ForHandledException(), false) { }

        internal Error(string errorClass, string message, IStackframe[] stackTrace, HandledState handledState, bool isAndroidJavaException)
        {
            ErrorClass = errorClass;
            ErrorMessage = message;
            _stacktrace = stackTrace.ToList();
            HandledState = handledState;
            IsAndroidJavaException = isAndroidJavaException;
            Type = COMPILATION_PLATFORM;
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

        public string Type 
        {
            get => this.Get(ERROR_TYPE_KEY) as string;
            set => this.AddToPayload(ERROR_TYPE_KEY, value);
        }
        public static bool ShouldSend(System.Exception exception)
        {
            var errorClass = exception.GetType().Name;
            if (errorClass != ANDROID_JAVA_EXCEPTION_CLASS && errorClass != NATIVE_ANDROID_ERROR_CLASS)
            {
                return true;
            }

            var match = Regex.Match(exception.StackTrace, NATIVE_ANDROID_MESSAGE_PATTERN, RegexOptions.Singleline);
            return !match.Success;
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
