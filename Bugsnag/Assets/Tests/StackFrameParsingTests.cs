using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
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
    }
}