using NUnit.Framework;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
    [TestFixture]
    class StackFrameParsingTests
    {
        [Test]
        public void ParseMethodNameWithColon()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "ReporterBehavior:LogCaughtException() (at /Users/gameserver/parky/Assets/ReporterBehavior.cs:58)"
            );
            Assert.AreEqual("ReporterBehavior:LogCaughtException()", stackframe.Method);
            Assert.AreEqual(58, stackframe.LineNumber);
            Assert.AreEqual("/Users/gameserver/parky/Assets/ReporterBehavior.cs", stackframe.File);
        }

        [Test]
        public void ParseMethodNameWithColonWithoutFileInfo()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.EventSystem:Update()"
            );
            Assert.AreEqual("UnityEngine.EventSystems.EventSystem:Update()", stackframe.Method);
            Assert.AreEqual(null, stackframe.LineNumber);
            Assert.AreEqual(null, stackframe.File);
        }

        [Test]
        public void ParseMethodNameWithSpace()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "ReporterBehavior.AssertionFailure () (at /Users/gameserver/parky/Assets/ReporterBehavior.cs:46)"
            );
            Assert.AreEqual("ReporterBehavior.AssertionFailure()", stackframe.Method);
            Assert.AreEqual(46, stackframe.LineNumber);
            Assert.AreEqual("/Users/gameserver/parky/Assets/ReporterBehavior.cs", stackframe.File);
        }

        [Test]
        public void ParseFilePathWithSpace()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "ReporterBehavior.AssertionFailure () (at /Users/game server/parky/Assets/ReporterBehavior.cs:46)"
            );
            Assert.AreEqual("ReporterBehavior.AssertionFailure()", stackframe.Method);
            Assert.AreEqual(46, stackframe.LineNumber);
            Assert.AreEqual("/Users/game server/parky/Assets/ReporterBehavior.cs", stackframe.File);
        }

        [Test]
        public void ParseMethodArgument()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "UnityEngine.UI.Button.OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/Button.cs:45)"
            );
            Assert.AreEqual("UnityEngine.UI.Button.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)", stackframe.Method);
            Assert.AreEqual(45, stackframe.LineNumber);
            Assert.AreEqual("/Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/Button.cs", stackframe.File);
        }

        [Test]
        public void ParseMultipleMethodArguments()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.ExecuteEvents.Execute (IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs:50)"
            );
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute(IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData)", stackframe.Method);
            Assert.AreEqual(50, stackframe.LineNumber);
            Assert.AreEqual("/Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs", stackframe.File);
        }

        [Test]
        public void ParseInterfaceMethod()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler] (UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs:261)"
            );
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerClickHandler](UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor)", stackframe.Method);
            Assert.AreEqual(261, stackframe.LineNumber);
            Assert.AreEqual("/Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs", stackframe.File);
        }

        [Test]
        public void ParseGenericMethod()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage(
              "UnityEngine.EventSystems.ExecuteEvents+EventFunction`1[T1].Invoke (.T1 handler, UnityEngine.EventSystems.BaseEventData eventData)"
            );
            Assert.AreEqual("UnityEngine.EventSystems.ExecuteEvents+EventFunction`1[T1].Invoke(.T1 handler, UnityEngine.EventSystems.BaseEventData eventData)", stackframe.Method);
            Assert.AreEqual(null, stackframe.LineNumber);
            Assert.AreEqual(null, stackframe.File);
        }

        [Test]
        public void ParseUnknownManagedToNative()
        {
            var stackframe = Payload.StackTraceLine.FromLogMessage("at (wrapper managed-to-native) Program.NativeMethod(Program/StructToMarshal)");
            Assert.AreEqual("(wrapper managed-to-native)Program.NativeMethod(Program/StructToMarshal)", stackframe.Method);

            stackframe = Payload.StackTraceLine.FromLogMessage("at (wrapper scoop - de - woop) SomeClass.SomeMethod(Program / Something else)");
            Assert.AreEqual("(wrapper scoop - de - woop)SomeClass.SomeMethod(Program / Something else)", stackframe.Method);

        }
    }
}
