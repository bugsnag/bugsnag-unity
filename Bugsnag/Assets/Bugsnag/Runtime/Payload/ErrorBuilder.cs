using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
    class ErrorBuilder
    {
        public ErrorBuilder(INativeClient client)
        {
            NativeClient = client;
        }
        private INativeClient NativeClient;

        private const string ANDROID_JAVA_EXCEPTION_CLASS = "AndroidJavaException";
        private const string ERROR_CLASS_MESSAGE_PATTERN = @"^(?<errorClass>\S+):\s+(?<message>.*)";

        /// <summary>
        /// Build a set of Bugsnag payload exceptions from a single exception by resolving
        /// the inner exceptions present.
        /// </summary>
        internal IEnumerable<Error> EnumerateFrom(System.Exception exception, System.Diagnostics.StackFrame[] alternativeStackTrace)
        {
            return FlattenAndReverseExceptionTree(exception).Select(e => FromSystemException(e, alternativeStackTrace));
        }

        /// <summary>
        /// Build a set of Bugsnag payload exceptions from a single exception by resolving
        /// the inner exceptions present.
        /// </summary>
        internal IEnumerable<Error> EnumerateFrom(System.Exception exception, string providedStackTrace)
        {
            return FlattenAndReverseExceptionTree(exception).Select(e => FromSystemException(e, providedStackTrace));
        }

        internal Error FromUnityLogMessage(UnityLogMessage logMessage, System.Diagnostics.StackFrame[] stackFrames, Severity severity)
        {
            return FromUnityLogMessage(logMessage, stackFrames, severity, false);
        }

        internal Error FromUnityLogMessage(UnityLogMessage logMessage, System.Diagnostics.StackFrame[] fallbackStackFrames, Severity severity, bool forceUnhandled)
        {
            var match = Regex.Match(logMessage.Condition, ERROR_CLASS_MESSAGE_PATTERN, RegexOptions.Singleline);

            var lines = new StackTrace(logMessage.StackTrace).StackTraceLines;
            if (lines.Length == 0)
            {
                lines = new StackTrace(fallbackStackFrames).StackTraceLines;
            }

            var handledState = forceUnhandled
              ? HandledState.ForUnhandledException()
              : HandledState.ForUnityLogMessage(severity);

            if (match.Success)
            {
                var errorClass = match.Groups["errorClass"].Value;
                var message = match.Groups["message"].Value.Trim();
                var isAndroidJavaException = false;
                // JVM exceptions in the main thread are handled by unity and require extra formatting
                if (errorClass == ANDROID_JAVA_EXCEPTION_CLASS)
                {
                    isAndroidJavaException = true;
                    var androidErrorData = ProcessAndroidError(message);
                    errorClass = androidErrorData[0];
                    message = androidErrorData[1];
                    lines = new StackTrace(logMessage.StackTrace, StackTraceFormat.AndroidJava).StackTraceLines;
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

        internal Error FromStringInfo(string name, string message, string stacktrace)
        {
            var stackFrames = new StackTrace(stacktrace).StackTraceLines;
            return new Error(name, message, stackFrames);
        }

        internal Error FromSystemException(System.Exception exception, string stackTrace)
        {
            var errorClass = exception.GetType().Name;
            var lines = new StackTrace(stackTrace).StackTraceLines;

            return new Error(errorClass, exception.Message, lines);
        }

        internal Error FromSystemException(System.Exception exception, System.Diagnostics.StackFrame[] alternativeStackTrace)
        {
            var errorClass = exception.GetType().Name;

            // JVM exceptions in the main thread are handled by unity and require extra formatting
            if (errorClass == ANDROID_JAVA_EXCEPTION_CLASS)
            {
                var androidErrorData = ProcessAndroidError(exception.Message);
                var androidErrorClass = androidErrorData[0];
                var androidErrorMessage = androidErrorData[1];
                var lines = new StackTrace(exception.StackTrace, StackTraceFormat.AndroidJava).StackTraceLines;
                return new Error(androidErrorClass, androidErrorMessage, lines, HandledState.ForUnhandledException(), true);
            }
            else
            {
                StackTraceLine[] lines;
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    lines = new StackTrace(exception.StackTrace).StackTraceLines;
                }
                else
                {
                    lines = new StackTrace(alternativeStackTrace).StackTraceLines;
                }
                return new Error(errorClass, exception.Message, lines);
            }

        }

        private string[] ProcessAndroidError(string originalMessage)
        {
            string message;
            string errorClass;
            var match = Regex.Match(originalMessage, ERROR_CLASS_MESSAGE_PATTERN, RegexOptions.Singleline);
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
                errorClass = originalMessage;
                message = string.Empty;
            }
            return new[] { errorClass, message };
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
}
