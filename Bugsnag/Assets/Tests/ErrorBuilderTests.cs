using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class ErrorBuilderTests
    {
        // Pass null NativeClient — only methods that don't call NativeClient.ToStackFrames are tested.
        private ErrorBuilder Builder() => new ErrorBuilder(null);

        // ---- FromStringInfo ----

        [Test]
        public void FromStringInfo_SetsErrorClassAndMessage()
        {
            var error = Builder().FromStringInfo("MyException", "something went wrong", "");
            Assert.AreEqual("MyException", error["errorClass"]);
            Assert.AreEqual("something went wrong", error["message"]);
        }

        [Test]
        public void FromStringInfo_EmptyStacktrace_ProducesEmptyStacktrace()
        {
            var error = Builder().FromStringInfo("Err", "msg", "");
            Assert.IsNotNull(error["stacktrace"]);
        }

        [Test]
        public void FromStringInfo_ParsedStacktrace_IsPopulated()
        {
            var stacktrace = "  at MyApp.MyClass.MyMethod () [0x00000] in MyClass.cs:10";
            var error = Builder().FromStringInfo("Err", "msg", stacktrace);
            var lines = error["stacktrace"] as System.Collections.Generic.IList<IStackframe>;
            Assert.IsNotNull(lines);
            Assert.Greater(lines.Count, 0);
        }

        // ---- FromSystemException(Exception, string) ----

        [Test]
        public void FromSystemException_String_SetsErrorClassFromExceptionType()
        {
            var ex = new ArgumentNullException("param");
            var error = Builder().FromSystemException(ex, "");
            Assert.AreEqual("ArgumentNullException", error["errorClass"]);
        }

        [Test]
        public void FromSystemException_String_SetsMessage()
        {
            var ex = new InvalidOperationException("test message");
            var error = Builder().FromSystemException(ex, "");
            Assert.AreEqual("test message", error["message"]);
        }

        [Test]
        public void FromSystemException_String_WithStackTrace_ParsesFrames()
        {
            var ex = new Exception("boom");
            var stackTrace = "  at SomeClass.SomeMethod () [0x00000] in SomeFile.cs:5";
            var error = Builder().FromSystemException(ex, stackTrace);
            var lines = error["stacktrace"] as IList<IStackframe>;
            Assert.Greater(lines.Count, 0);
        }

        // ---- EnumerateFrom(Exception, string) ----

        [Test]
        public void EnumerateFrom_SingleException_ReturnsOneError()
        {
            var ex = new Exception("single");
            var errors = Builder().EnumerateFrom(ex, "").ToList();
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Exception", errors[0]["errorClass"]);
        }

        [Test]
        public void EnumerateFrom_NestedInnerException_ReturnsBothErrors()
        {
            var inner = new ArgumentException("inner");
            var outer = new InvalidOperationException("outer", inner);
            var errors = Builder().EnumerateFrom(outer, "").ToList();
            // outer + inner = 2 errors
            Assert.AreEqual(2, errors.Count);
        }

        [Test]
        public void EnumerateFrom_InnerExceptionFirst_ThenOuter()
        {
            var inner = new ArgumentException("inner");
            var outer = new InvalidOperationException("outer", inner);
            var errors = Builder().EnumerateFrom(outer, "").ToList();
            // FlattenAndReverseExceptionTree yields outer first, then inner
            Assert.AreEqual("InvalidOperationException", errors[0]["errorClass"]);
            Assert.AreEqual("ArgumentException", errors[1]["errorClass"]);
        }

        // ---- FromUnityLogMessage ----

        [Test]
        public void FromUnityLogMessage_MatchesErrorClassPattern_SetsClassAndMessage()
        {
            var msg = new UnityLogMessage("SomeException: the detail here", "", LogType.Error);
            var error = Builder().FromUnityLogMessage(msg, new System.Diagnostics.StackFrame[0], Severity.Error);
            Assert.AreEqual("SomeException", error["errorClass"]);
            Assert.AreEqual("the detail here", error["message"]);
        }

        [Test]
        public void FromUnityLogMessage_NoPattern_FallsBackToUnityLogPrefix()
        {
            var msg = new UnityLogMessage("just a plain log message", "", LogType.Warning);
            var error = Builder().FromUnityLogMessage(msg, new System.Diagnostics.StackFrame[0], Severity.Warning);
            Assert.IsTrue(error["errorClass"].ToString().StartsWith("UnityLog"), error["errorClass"].ToString());
        }

        [Test]
        public void FromUnityLogMessage_IsHandled_WhenNotForced()
        {
            var msg = new UnityLogMessage("SomeError: detail", "", LogType.Error);
            var error = Builder().FromUnityLogMessage(msg, new System.Diagnostics.StackFrame[0], Severity.Error);
            // ForUnityLogMessage → handled = true → unhandled = false
            Assert.AreEqual(false, error.HandledState["unhandled"]);
        }

        [Test]
        public void FromUnityLogMessage_ForceUnhandled_SetsUnhandledTrue()
        {
            var msg = new UnityLogMessage("SomeError: detail", "", LogType.Error);
            var error = Builder().FromUnityLogMessage(msg, new System.Diagnostics.StackFrame[0], Severity.Error, forceUnhandled: true);
            Assert.AreEqual(true, error.HandledState["unhandled"]);
        }
    }

    [TestFixture]
    public class ExceptionTests
    {
        private ErrorBuilder errorBuilder = new ErrorBuilder(new NativeClient(new Configuration("X")));

        [Test]
        public void ParseExceptionFromLogMessage()
        {
            string condition = "IndexOutOfRangeException: Array index is out of range.";
            string stacktrace = @"ReporterBehavior.AssertionFailure () [0x00000] in <filename unknown>:0
   UnityEngine.Events.InvokableCall.Invoke () [0x00000] in <filename unknown>:0
   UnityEngine.Events.UnityEvent.Invoke () [0x00000] in <filename unknown>:0
   UnityEngine.UI.Button.Press () [0x00000] in <filename unknown>:0
   UnityEngine.UI.Button.OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData) [0x00000] in <filename unknown>:0
   UnityEngine.EventSystems.ExecuteEvents.Execute (IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) [0x00000] in <filename unknown>:0
   UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler] (UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor) [0x00000] in <filename unknown>:0";
            var logType = LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);

            Assert.IsTrue(Error.ShouldSend(log));

            var exception = errorBuilder.FromUnityLogMessage(log, new System.Diagnostics.StackFrame[] { }, Severity.Info);
            var stack = exception.Stacktrace.ToList();

            Assert.AreEqual(7, stack.Count);
            Assert.AreEqual("IndexOutOfRangeException", exception.ErrorClass);
            Assert.AreEqual("Array index is out of range.", exception.ErrorMessage);
            Assert.AreEqual("ReporterBehavior.AssertionFailure()", stack[0].Method);
            Assert.AreEqual("<filename unknown>", stack[0].File);
            Assert.AreEqual(0, stack[0].LineNumber);
            Assert.AreEqual("UnityEngine.Events.InvokableCall.Invoke()", stack[1].Method);
            Assert.AreEqual("<filename unknown>", stack[1].File);
            Assert.AreEqual(0, stack[1].LineNumber);
            Assert.AreEqual("UnityEngine.Events.UnityEvent.Invoke()", stack[2].Method);
            Assert.AreEqual("<filename unknown>", stack[2].File);
            Assert.AreEqual(0, stack[2].LineNumber);
            Assert.AreEqual("UnityEngine.UI.Button.Press()", stack[3].Method);
            Assert.AreEqual("<filename unknown>", stack[3].File);
            Assert.AreEqual(0, stack[3].LineNumber);
            Assert.AreEqual("UnityEngine.UI.Button.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)", stack[4].Method);
            Assert.AreEqual("<filename unknown>", stack[4].File);
            Assert.AreEqual(0, stack[4].LineNumber);
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute(IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData)", stack[5].Method);
            Assert.AreEqual("<filename unknown>", stack[5].File);
            Assert.AreEqual(0, stack[5].LineNumber);
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler](UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor)", stack[6].Method);
            Assert.AreEqual("<filename unknown>", stack[6].File);
            Assert.AreEqual(0, stack[6].LineNumber);
        }

        [Test]
        public void ParseDuplicateAndroidExceptionFromLogMessage()
        {
            string condition = "AndroidJavaException: java.lang.Error";
            string stacktrace = @"java.lang.Error: signal 11 (SIGSEGV), code 1 (SEGV_MAPERR), fault addr 0102192a9accb1876a
libunity.0033c25b(Unknown:-2)
libunity.003606e3(Unknown:-2)
app_process64.000d1b11(Unknown:-2)";
            var logType = LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);

            Assert.IsFalse(Error.ShouldSend(log));
        }

        [Test]
        public void ParseAndroidExceptionFromLogMessage()
        {
            string condition = "AndroidJavaException: java.lang.IllegalArgumentException";
            string stacktrace = @"java.lang.IllegalArgumentException
com.example.bugsnagcrashplugin.CrashHelper.UnhandledCrash(CrashHelper.java:11)
com.unity3d.player.UnityPlayer.nativeRender(Native Method)";
            var logType = LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);

            Assert.IsTrue(Error.ShouldSend(log));
            var exception = errorBuilder.FromUnityLogMessage(log, new System.Diagnostics.StackFrame[] { }, Severity.Warning);
            var stack = exception.Stacktrace.ToList();

            Assert.AreEqual("java.lang.IllegalArgumentException", exception.ErrorClass);
            Assert.IsTrue(string.IsNullOrEmpty(exception.ErrorMessage));
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual("com.example.bugsnagcrashplugin.CrashHelper.UnhandledCrash()", stack[0].Method);
            Assert.AreEqual("CrashHelper.java", stack[0].File);
            Assert.AreEqual(11, stack[0].LineNumber);
            Assert.AreEqual("com.unity3d.player.UnityPlayer.nativeRender()", stack[1].Method);
            Assert.AreEqual("Native Method", stack[1].File);
            Assert.AreEqual(null, stack[1].LineNumber);
        }

        [Test]
        public void ParseAndroidExceptionAndMessageFromLogMessage()
        {
            string condition = "AndroidJavaException: java.lang.ArrayIndexOutOfBoundsException: length=2; index=2";
            string stacktrace = @"java.lang.ArrayIndexOutOfBoundsException: length=2; index=2
com.example.bugsnagcrashplugin.CrashHelper.UnhandledCrash(CrashHelper.java:11)
com.unity3d.player.UnityPlayer.nativeRender(Native Method)
com.unity3d.player.UnityPlayer.c(Unknown Source:0)
com.unity3d.player.UnityPlayer$e$2.queueIdle(Unknown Source:72)
android.os.MessageQueue.next(MessageQueue.java:395)
android.os.Looper.loop(Looper.java:160)
com.unity3d.player.UnityPlayer$e.run(Unknown Source:32)
UnityEngine.AndroidJNISafe.CheckException()
UnityEngine.AndroidJNISafe.CallStaticVoidMethod(IntPtr clazz, IntPtr methodID, UnityEngine.jvalue[] args)
UnityEngine.AndroidJavaObject._CallStatic(System.String methodName, System.Object[] args)
UnityEngine.EventSystems.ExecuteEvents.Execute(IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData)
UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functorUnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler]()
UnityEngine.EventSystems.EventSystem:Update()";
            var logType = LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);

            Assert.IsTrue(Error.ShouldSend(log));

            var exception = errorBuilder.FromUnityLogMessage(log, new System.Diagnostics.StackFrame[] { }, Severity.Warning);
            var stack = exception.Stacktrace.ToList();

            Assert.AreEqual(13, stack.Count);
            Assert.AreEqual("java.lang.ArrayIndexOutOfBoundsException", exception.ErrorClass);
            Assert.AreEqual("length=2; index=2", exception.ErrorMessage);
            Assert.AreEqual("com.example.bugsnagcrashplugin.CrashHelper.UnhandledCrash()", stack[0].Method);
            Assert.AreEqual("CrashHelper.java", stack[0].File);
            Assert.AreEqual(11, stack[0].LineNumber);
            Assert.AreEqual("com.unity3d.player.UnityPlayer.nativeRender()", stack[1].Method);
            Assert.AreEqual("Native Method", stack[1].File);
            Assert.AreEqual(null, stack[1].LineNumber);
            Assert.AreEqual("com.unity3d.player.UnityPlayer.c()", stack[2].Method);
            Assert.AreEqual("Unknown Source", stack[2].File);
            Assert.AreEqual(0, stack[2].LineNumber);
            Assert.AreEqual("com.unity3d.player.UnityPlayer$e$2.queueIdle()", stack[3].Method);
            Assert.AreEqual("Unknown Source", stack[3].File);
            Assert.AreEqual(72, stack[3].LineNumber);
            Assert.AreEqual("android.os.MessageQueue.next()", stack[4].Method);
            Assert.AreEqual("MessageQueue.java", stack[4].File);
            Assert.AreEqual(395, stack[4].LineNumber);
            Assert.AreEqual("android.os.Looper.loop()", stack[5].Method);
            Assert.AreEqual("Looper.java", stack[5].File);
            Assert.AreEqual(160, stack[5].LineNumber);
            Assert.AreEqual("com.unity3d.player.UnityPlayer$e.run()", stack[6].Method);
            Assert.AreEqual("Unknown Source", stack[6].File);
            Assert.AreEqual(32, stack[6].LineNumber);
            Assert.AreEqual("UnityEngine.AndroidJNISafe.CheckException()", stack[7].Method);
            Assert.AreEqual(null, stack[7].File);
            Assert.AreEqual(null, stack[7].LineNumber);
            Assert.AreEqual("UnityEngine.AndroidJNISafe.CallStaticVoidMethod(IntPtr clazz, IntPtr methodID, UnityEngine.jvalue[] args)", stack[8].Method);
            Assert.AreEqual(null, stack[8].File);
            Assert.AreEqual(null, stack[8].LineNumber);
            Assert.AreEqual("UnityEngine.AndroidJavaObject._CallStatic(System.String methodName, System.Object[] args)", stack[9].Method);
            Assert.AreEqual(null, stack[9].File);
            Assert.AreEqual(null, stack[9].LineNumber);
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute(IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData)", stack[10].Method);
            Assert.AreEqual(null, stack[10].File);
            Assert.AreEqual(null, stack[10].LineNumber);
            Assert.AreEqual("UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functorUnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler]()", stack[11].Method);
            Assert.AreEqual(null, stack[11].File);
            Assert.AreEqual(null, stack[11].LineNumber);
            Assert.AreEqual("UnityEngine.EventSystems.EventSystem:Update()", stack[12].Method);
            Assert.AreEqual(null, stack[12].File);
            Assert.AreEqual(null, stack[12].LineNumber);
        }
    }

    [TestFixture]
    public class StackFrameParsingTests
    {
        [Test]
        public void ParseMethodWithAt()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "at UnityEngine.Events.InvokableCall.Invoke () [0x00010] in /Users/bokken/build/output/unity/unity/Runtime/Export/UnityEvent/UnityEvent.cs:178"
            );
            Assert.AreEqual("UnityEngine.Events.InvokableCall.Invoke()", stackframe.Method);
        }

        [Test]
        public void ParseMethodNameWithColon()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "ReporterBehavior:LogCaughtException() (at /Users/gameserver/parky/Assets/ReporterBehavior.cs:58)"
            );
            Assert.AreEqual("ReporterBehavior:LogCaughtException()", stackframe.Method);
            Assert.AreEqual(58, stackframe.LineNumber);
            Assert.AreEqual("/Users/gameserver/parky/Assets/ReporterBehavior.cs", stackframe.File);
        }

        [Test]
        public void ParseMethodNameWithColonWithoutFileInfo()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.EventSystem:Update()"
            );
            Assert.AreEqual("UnityEngine.EventSystems.EventSystem:Update()", stackframe.Method);
            Assert.IsNull(stackframe.LineNumber);
            Assert.IsNull(stackframe.File);
        }

        [Test]
        public void ParseMethodNameWithSpace()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "ReporterBehavior.AssertionFailure () (at /Users/gameserver/parky/Assets/ReporterBehavior.cs:46)"
            );
            Assert.AreEqual("ReporterBehavior.AssertionFailure()", stackframe.Method);
            Assert.AreEqual(46, stackframe.LineNumber);
            Assert.AreEqual("/Users/gameserver/parky/Assets/ReporterBehavior.cs", stackframe.File);
        }

        [Test]
        public void ParseFilePathWithSpace()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "ReporterBehavior.AssertionFailure () (at /Users/game server/parky/Assets/ReporterBehavior.cs:46)"
            );
            Assert.AreEqual("ReporterBehavior.AssertionFailure()", stackframe.Method);
            Assert.AreEqual(46, stackframe.LineNumber);
            Assert.AreEqual("/Users/game server/parky/Assets/ReporterBehavior.cs", stackframe.File);
        }

        [Test]
        public void ParseMethodArgument()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "UnityEngine.UI.Button.OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/Button.cs:45)"
            );
            Assert.AreEqual("UnityEngine.UI.Button.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)", stackframe.Method);
            Assert.AreEqual(45, stackframe.LineNumber);
            Assert.AreEqual("/Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/Button.cs", stackframe.File);
        }

        [Test]
        public void ParseMultipleMethodArguments()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.ExecuteEvents.Execute (IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs:50)"
            );
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute(IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData)", stackframe.Method);
            Assert.AreEqual(50, stackframe.LineNumber);
            Assert.AreEqual("/Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs", stackframe.File);
        }

        [Test]
        public void ParseInterfaceMethod()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler] (UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs:261)"
            );
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler](UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor)", stackframe.Method);
            Assert.AreEqual(261, stackframe.LineNumber);
            Assert.AreEqual("/Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs", stackframe.File);
        }

        [Test]
        public void ParseGenericMethod()
        {
            var stackframe = StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.ExecuteEvents+EventFunction`1[T1].Invoke (.T1 handler, UnityEngine.EventSystems.BaseEventData eventData)"
            );
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents+EventFunction`1[T1].Invoke(.T1 handler, UnityEngine.EventSystems.BaseEventData eventData)", stackframe.Method);
            Assert.IsNull(stackframe.LineNumber);
            Assert.IsNull(stackframe.File);
        }

        [Test]
        public void ParseUnknownManagedToNative()
        {
            var stackframe = StackTraceLine.FromLogMessage("at (wrapper managed-to-native) Program.NativeMethod(Program/StructToMarshal)");
            Assert.AreEqual("(wrapper managed-to-native) Program.NativeMethod(Program/StructToMarshal)", stackframe.Method);

            stackframe = StackTraceLine.FromLogMessage("at (wrapper scoop-de-woop) SomeClass.SomeMethod(Program / Something else)");
            Assert.AreEqual("(wrapper scoop-de-woop) SomeClass.SomeMethod(Program / Something else)", stackframe.Method);
        }

        [Test]
        public void ParseAndroidMethod()
        {
            var stackframe = StackTraceLine.FromAndroidJavaMessage("at com.example.lib.BugsnagCrash.throwJvmException(BugsnagCrash.java:14)");
            Assert.AreEqual("com.example.lib.BugsnagCrash.throwJvmException()", stackframe.Method);
            Assert.AreEqual("BugsnagCrash.java", stackframe.File);
            Assert.AreEqual(14, stackframe.LineNumber);
        }

        [Test]
        public void ConvertSystemStackTraceFrames()
        {
            var systemStackTrace = new System.Diagnostics.StackTrace(true);
            var systemFrames = systemStackTrace.GetFrames();

            Assert.IsNotNull(systemFrames, "Expected non-null StackFrame array from System.Diagnostics.StackTrace");
            Assert.IsNotEmpty(systemFrames, "Expected at least one frame in the captured stack trace");

            foreach (var frame in systemFrames)
            {
                var expectedMethod = new Method(frame.GetMethod()).DisplayName();
                var expectedFile = frame.GetFileName();
                var expectedLine = frame.GetFileLineNumber();

                var bugsnagFrame = StackTraceLine.FromStackFrame(frame);

                Assert.AreEqual(expectedMethod, bugsnagFrame.Method, "Method name did not match for frame {0}", expectedMethod);
                Assert.AreEqual(expectedFile, bugsnagFrame.File, "File path did not match for method {0}", expectedMethod);
                Assert.AreEqual(expectedLine, bugsnagFrame.LineNumber, "Line number did not match for method {0}", expectedMethod);
            }
        }

        [Test]
        public void MixedAndroidStackTrace()
        {
            var stackTraceMessage = "java.lang.ClassNotFoundException: java.lang.String\n" +
                "  at java.lang.Class.forName(Class.java:453)\n" +
                "  at UnityEngine.AndroidJavaClass._AndroidJavaClass (System.String className) (at <00000000000000000000000000000000>:0)\n";

            var lines = new PayloadStackTrace(stackTraceMessage, StackTraceFormat.AndroidJava).StackTraceLines;

            Assert.AreEqual(2, lines.Length);

            var javaFrame = lines[0];
            Assert.AreEqual("java.lang.Class.forName()", javaFrame.Method);
            Assert.AreEqual("Class.java", javaFrame.File);
            Assert.AreEqual(453, javaFrame.LineNumber);

            var standardFrame = lines[1];
            Assert.AreEqual("UnityEngine.AndroidJavaClass._AndroidJavaClass(System.String className)", standardFrame.Method);
            Assert.AreEqual("<00000000000000000000000000000000>", standardFrame.File);
            Assert.AreEqual(0, standardFrame.LineNumber);
        }
    }

    [TestFixture]
    public class PayloadStackTraceTests
    {
        [Test]
        public void StackTrace_FromString_ParsesLines()
        {
            var st = new PayloadStackTrace("  at MyClass.MyMethod () [0x0] in File.cs:10");
            Assert.Greater(st.StackTraceLines.Length, 0);
        }

        [Test]
        public void StackTrace_FromEmptyString_ProducesEmptyLines()
        {
            var st = new PayloadStackTrace("");
            Assert.AreEqual(0, st.StackTraceLines.Length);
        }

        [Test]
        public void StackTrace_MultipleLines_ParsesAll()
        {
            var trace = "  at A.B () [0x0] in A.cs:1\n  at C.D () [0x0] in C.cs:2";
            var st = new PayloadStackTrace(trace);
            Assert.AreEqual(2, st.StackTraceLines.Length);
        }

        [Test]
        public void StackTrace_Enumeration_YieldsLines()
        {
            var st = new PayloadStackTrace("  at A.B () [0x0] in A.cs:1");
            int count = 0;
            foreach (var line in st) count++;
            Assert.AreEqual(1, count);
        }

        [Test]
        public void StackTrace_AndroidJavaFormat_ParsesCorrectly()
        {
            var trace = "java.lang.Exception: boom\n  at com.example.MyClass.method(MyClass.java:5)";
            var st = new PayloadStackTrace(trace, StackTraceFormat.AndroidJava);
            Assert.AreEqual(1, st.StackTraceLines.Length);
        }

        [Test]
        public void StackTrace_FromStackFrames_ProducesLines()
        {
            var frames = new System.Diagnostics.StackTrace(true).GetFrames() ?? new System.Diagnostics.StackFrame[0];
            var st = new PayloadStackTrace(frames);
            Assert.IsNotNull(st.StackTraceLines);
        }

        [Test]
        public void StackTrace_NonGenericEnumerator_YieldsLines()
        {
            var st = new PayloadStackTrace("  at A.B () [0x0] in A.cs:1");
            var enumerator = ((System.Collections.IEnumerable)st).GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext()) count++;
            Assert.AreEqual(1, count);
        }

        [Test]
        public void StackTrace_FiltersRethrowMarker()
        {
            // Test that rethrow markers are filtered out to prevent IL2CPP off-by-one alignment issues
            var trace = @"  at AnimationTask.DoTick () [0x0] in AnimationTask.cs:13
--- End of stack trace from previous location where exception was thrown ---
  at TestRethrow.ThrowNullReferenceAsync() [0x0] in TestRethrow.cs:77
  at ClassManager.Update () [0x0] in ClassManager.cs:35";
            var st = new PayloadStackTrace(trace);
            // Should only contain 3 frames, not 4 (rethrow marker should be filtered)
            Assert.AreEqual(3, st.StackTraceLines.Length);
            Assert.AreEqual("AnimationTask.DoTick()", st.StackTraceLines[0].Method);
            Assert.AreEqual("TestRethrow.ThrowNullReferenceAsync()", st.StackTraceLines[1].Method);
            Assert.AreEqual("ClassManager.Update()", st.StackTraceLines[2].Method);
        }

        [Test]
        public void StackTrace_FiltersMultipleRethrowMarkers()
        {
            var trace = @"  at Method1 () [0x0] in File1.cs:1
--- End of stack trace from previous location where exception was thrown ---
  at Method2 () [0x0] in File2.cs:2
--- End of inner exception stack trace ---
  at Method3 () [0x0] in File3.cs:3";
            var st = new PayloadStackTrace(trace);
            Assert.AreEqual(3, st.StackTraceLines.Length);
            Assert.AreEqual("Method1()", st.StackTraceLines[0].Method);
            Assert.AreEqual("Method2()", st.StackTraceLines[1].Method);
            Assert.AreEqual("Method3()", st.StackTraceLines[2].Method);
        }
    }

    [TestFixture]
    public class ErrorMoreTests
    {
        [Test]
        public void Constructor_IsAndroidJavaException_True_SetsFlag()
        {
            var error = new Error("Err", "msg",
                new IStackframe[0], HandledState.ForHandledException(), true);
            Assert.IsTrue(error.IsAndroidJavaException);
        }

        [Test]
        public void Constructor_IsAndroidJavaException_False_SetsFlag()
        {
            var error = new Error("Err", "msg",
                new IStackframe[0], HandledState.ForHandledException(), false);
            Assert.IsFalse(error.IsAndroidJavaException);
        }

        [Test]
        public void ShouldSend_UnityLogMessage_WithStackTrace_NonAndroid_ReturnsTrue()
        {
            var msg = new UnityLogMessage("NullReferenceException: object", "at SomeClass()", LogType.Exception);
            Assert.IsTrue(Error.ShouldSend(msg));
        }

        [Test]
        public void ShouldSend_UnityLogMessage_PlainMessage_NoColon_ReturnsTrue()
        {
            var msg = new UnityLogMessage("some plain log", "at SomeClass()", LogType.Log);
            Assert.IsTrue(Error.ShouldSend(msg));
        }
    }

    [TestFixture]
    public class StackTraceLineTests
    {
        [Test]
        public void FromLogMessage_StandardFormat_ParsesMethod()
        {
            var line = StackTraceLine.FromLogMessage("  at MyApp.MyClass.MyMethod () [0x00000] in MyFile.cs:42");
            Assert.IsNotNull(line.Method);
            Assert.IsTrue(line.Method.Contains("MyMethod"), line.Method);
        }

        [Test]
        public void FromLogMessage_StandardFormat_ParsesFile()
        {
            var line = StackTraceLine.FromLogMessage("  at MyApp.MyClass.MyMethod () [0x00000] in MyFile.cs:42");
            Assert.IsNotNull(line.File);
            Assert.IsTrue(line.File.Contains("MyFile.cs"), line.File);
        }

        [Test]
        public void FromLogMessage_StandardFormat_ParsesLineNumber()
        {
            var line = StackTraceLine.FromLogMessage("  at MyApp.MyClass.MyMethod () [0x00000] in MyFile.cs:42");
            Assert.AreEqual(42, line.LineNumber);
        }

        [Test]
        public void FromLogMessage_UnparsedMessage_StillReturnsLine()
        {
            var line = StackTraceLine.FromLogMessage("some unparseable string");
            Assert.IsNotNull(line);
            Assert.AreEqual("some unparseable string", line.Method);
        }

        [Test]
        public void FromLogMessage_NoLineNumber_LineNumberIsNull()
        {
            var line = StackTraceLine.FromLogMessage("  at SomeMethod () [0x00000]");
            Assert.IsNull(line.LineNumber);
        }

        [Test]
        public void FromAndroidJavaMessage_JavaFormat_ParsesMethod()
        {
            var line = StackTraceLine.FromAndroidJavaMessage("  at com.example.MyClass.myMethod(MyClass.java:10)");
            Assert.IsNotNull(line.Method);
            Assert.IsTrue(line.Method.Contains("myMethod"), line.Method);
        }

        [Test]
        public void FromAndroidJavaMessage_JavaFormat_ParsesLineNumber()
        {
            var line = StackTraceLine.FromAndroidJavaMessage("  at com.example.MyClass.myMethod(MyClass.java:10)");
            Assert.AreEqual(10, line.LineNumber);
        }

        [Test]
        public void File_SetAndGet()
        {
            var line = StackTraceLine.FromLogMessage("raw");
            line.File = "new/file.cs";
            Assert.AreEqual("new/file.cs", line.File);
        }

        [Test]
        public void LineNumber_SetAndGet()
        {
            var line = StackTraceLine.FromLogMessage("raw");
            line.LineNumber = 99;
            Assert.AreEqual(99, line.LineNumber);
        }

        [Test]
        public void Method_SetAndGet()
        {
            var line = StackTraceLine.FromLogMessage("raw");
            line.Method = "NewMethod()";
            Assert.AreEqual("NewMethod()", line.Method);
        }

        [Test]
        public void FrameAddress_SetAndGet()
        {
            var line = StackTraceLine.FromLogMessage("raw");
            line.FrameAddress = "0xABCD";
            Assert.AreEqual("0xABCD", line.FrameAddress);
        }

        [Test]
        public void LoadAddress_SetAndGet()
        {
            var line = StackTraceLine.FromLogMessage("raw");
            line.LoadAddress = "0x1000";
            Assert.AreEqual("0x1000", line.LoadAddress);
        }
    }

    [TestFixture]
    public class StackTraceLineAdditionalTests
    {
        [Test]
        public void File_Setter_UpdatesFile()
        {
            var line = new StackTraceLine("original.cs", 1, "Method()");
            line.File = "updated.cs";
            Assert.AreEqual("updated.cs", line.File);
        }

        [Test]
        public void LineNumber_Setter_UpdatesLineNumber()
        {
            var line = new StackTraceLine("f.cs", 1, "M()");
            line.LineNumber = 99;
            Assert.AreEqual(99, line.LineNumber);
        }

        [Test]
        public void Method_Setter_UpdatesMethod()
        {
            var line = new StackTraceLine("f.cs", 1, "OldMethod()");
            line.Method = "NewMethod()";
            Assert.AreEqual("NewMethod()", line.Method);
        }

        [Test]
        public void FrameAddress_SetAndGet_Works()
        {
            var line = new StackTraceLine();
            line.FrameAddress = "0xDEADBEEF";
            Assert.AreEqual("0xDEADBEEF", line.FrameAddress);
        }

        [Test]
        public void FrameAddress_DefaultIsNull()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.FrameAddress);
        }

        [Test]
        public void FromAndroidJavaMessage_JavaFormat_ParsesCorrectly()
        {
            var line = StackTraceLine.FromAndroidJavaMessage(
                "at com.example.MyClass.doSomething(MyClass.java:42)");
            Assert.IsNotNull(line);
            Assert.AreEqual(42, line.LineNumber);
            StringAssert.Contains("MyClass.java", line.File);
        }

        [Test]
        public void FromAndroidJavaMessage_NoLineNumber_LineNumberIsNull()
        {
            var line = StackTraceLine.FromAndroidJavaMessage(
                "at com.example.MyClass.nativeMethod(Native Method)");
            Assert.IsNotNull(line);
        }

        [Test]
        public void FromAndroidJavaMessage_FallsBackToFromLogMessage_WhenNotJavaFormat()
        {
            var line = StackTraceLine.FromAndroidJavaMessage(
                "  at MyNamespace.MyClass.Method () [0x00000] in /path/to/File.cs:10");
            Assert.IsNotNull(line);
        }

        [Test]
        public void FromStackFrame_CurrentFrame_ReturnsLine()
        {
            var frame = new System.Diagnostics.StackFrame(true);
            var line = StackTraceLine.FromStackFrame(frame);
            Assert.IsNotNull(line);
        }

        [Test]
        public void StackTraceLine_DictConstructor_CopiesData()
        {
            var data = new Dictionary<string, object>
            {
                ["file"] = "test.cs",
                ["lineNumber"] = 5,
                ["method"] = "TestMethod()"
            };
            var line = new StackTraceLine(data);
            Assert.AreEqual("test.cs", line.File);
            Assert.AreEqual("TestMethod()", line.Method);
        }

        [Test]
        public void FromLogMessage_UnknownFormat_ReturnsLineWithMessage()
        {
            var line = StackTraceLine.FromLogMessage("some unknown log message");
            Assert.IsNotNull(line);
        }

        [Test]
        public void MachoLoadAddress_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.MachoLoadAddress);
            line.MachoLoadAddress = "0x1000";
            Assert.AreEqual("0x1000", line.MachoLoadAddress);
        }

        [Test]
        public void MachoFile_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.MachoFile);
            line.MachoFile = "MyApp.app";
            Assert.AreEqual("MyApp.app", line.MachoFile);
        }

        [Test]
        public void MachoUuid_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.MachoUuid);
            line.MachoUuid = "AABBCCDD-1122-3344-5566-778899AABBCC";
            Assert.AreEqual("AABBCCDD-1122-3344-5566-778899AABBCC", line.MachoUuid);
        }

        [Test]
        public void InProject_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.InProject);
            line.InProject = true;
            Assert.AreEqual(true, line.InProject);
            line.InProject = false;
            Assert.AreEqual(false, line.InProject);
        }

        [Test]
        public void CodeIdentifier_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.CodeIdentifier);
            line.CodeIdentifier = "com.example.Module";
            Assert.AreEqual("com.example.Module", line.CodeIdentifier);
        }

        [Test]
        public void Type_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.Type);
            line.Type = "cocoa";
            Assert.AreEqual("cocoa", line.Type);
        }

        [Test]
        public void IsLr_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.IsLr);
            line.IsLr = true;
            Assert.AreEqual(true, line.IsLr);
        }

        [Test]
        public void IsPc_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.IsPc);
            line.IsPc = true;
            Assert.AreEqual(true, line.IsPc);
        }

        [Test]
        public void MachoVmAddress_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.MachoVmAddress);
            line.MachoVmAddress = "0x200000000";
            Assert.AreEqual("0x200000000", line.MachoVmAddress);
        }

        [Test]
        public void SymbolAddress_SetAndGet()
        {
            var line = new StackTraceLine();
            Assert.IsNull(line.SymbolAddress);
            line.SymbolAddress = "0xDEADBEEF";
            Assert.AreEqual("0xDEADBEEF", line.SymbolAddress);
        }
    }
}
