using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
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
            var logType = UnityEngine.LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);
            Assert.IsTrue(Error.ShouldSend(log));

            var exception = Error.FromUnityLogMessage(log, new System.Diagnostics.StackFrame[] { }, Severity.Info);
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

        [TestMethod]
        public void ParseDuplicateAndroidExceptionFromLogMessage()
        {
            string condition = "AndroidJavaException: java.lang.Error";
            string stacktrace = @"java.lang.Error: signal 11 (SIGSEGV), code 1 (SEGV_MAPERR), fault addr 0102192a9accb1876a
libunity.0033c25b(Unknown:-2)
libunity.003606e3(Unknown:-2)
app_process64.000d1b11(Unknown:-2)";
            var logType = UnityEngine.LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);
            Assert.IsFalse(Error.ShouldSend(log));
        }

        [TestMethod]
        public void ParseAndroidExceptionFromLogMessage()
        {
            string condition = "AndroidJavaException: java.lang.IllegalArgumentException";
            string stacktrace = @"java.lang.IllegalArgumentException
com.example.bugsnagcrashplugin.CrashHelper.UnhandledCrash(CrashHelper.java:11)
com.unity3d.player.UnityPlayer.nativeRender(Native Method)";
            var logType = UnityEngine.LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);
            Assert.IsTrue(Error.ShouldSend(log));
            var exception = Error.FromUnityLogMessage(log, new System.Diagnostics.StackFrame[] { }, Severity.Warning);
            var stack = exception.Stacktrace.ToList();
            Assert.AreEqual("java.lang.IllegalArgumentException", exception.ErrorClass);
            Assert.IsTrue(System.String.IsNullOrEmpty(exception.ErrorMessage));
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual("com.example.bugsnagcrashplugin.CrashHelper.UnhandledCrash()", stack[0].Method);
            Assert.AreEqual("CrashHelper.java", stack[0].File);
            Assert.AreEqual(11, stack[0].LineNumber);
            Assert.AreEqual("com.unity3d.player.UnityPlayer.nativeRender()", stack[1].Method);
            Assert.AreEqual("Native Method", stack[1].File);
            Assert.AreEqual(null, stack[1].LineNumber);
        }

        [TestMethod]
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
            var logType = UnityEngine.LogType.Error;
            var log = new UnityLogMessage(condition, stacktrace, logType);
            Assert.IsTrue(Error.ShouldSend(log));

            var exception = Error.FromUnityLogMessage(log, new System.Diagnostics.StackFrame[] { }, Severity.Warning);
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
}
