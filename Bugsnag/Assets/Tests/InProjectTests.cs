using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class InProjectTests
    {
        private static readonly string[] Defaults = Configuration.DefaultExcludedNamespaces;

        private static StackTraceLine Frame(string method) => new StackTraceLine("", null, method);

        // ---- Name parsing ----

        [Test]
        public void Parsing_IgnoresArgsWrapperAndWhitespace()
        {
            var include = new[] { "MyGame" };
            // Prefix appearing only inside the argument list must not match
            Assert.IsFalse(InProjectResolver.IsInProject("Other.Thing.Do(MyGame.Player)", include, Defaults));
            Assert.IsTrue(InProjectResolver.IsInProject("  MyGame.Player.Die ()", include, Defaults));
            Assert.IsTrue(InProjectResolver.IsInProject("(wrapper managed-to-native) MyGame.Native.Call(intptr)", include, Defaults));
            // Whole method is the namespace with no args
            Assert.IsTrue(InProjectResolver.IsInProject("MyGame", include, Defaults));
            // Prefix longer than the name never matches
            Assert.IsFalse(InProjectResolver.IsInProject("My", include, Defaults));
        }

        // ---- Exclude-based defaults ----

        [TestCase("UnityEngine.Debug:Log(Object)")]
        [TestCase("UnityEditor.EditorApplication.Internal_CallUpdateFunctions()")]
        [TestCase("Unity.Collections.NativeArray`1.get_Item(Int32)")]
        [TestCase("System.Threading.Tasks.Task.Run()")]
        [TestCase("Mono.Runtime.Foo()")]
        [TestCase("Cysharp.Threading.Tasks.UniTask+<>c.<Run>b__0()")]
        [TestCase("DG.Tweening.Tween.DoGoto(Single)")]
        [TestCase("TMPro.TMP_Text.SetText(String)")]
        [TestCase("BugsnagUnity.Client.Notify(Exception)")]
        [TestCase("(wrapper managed-to-native) System.Object.__icall_wrapper(intptr)")]
        public void Defaults_ExcludeEngineAndThirdParty(string method)
        {
            Assert.IsFalse(InProjectResolver.IsInProject(method, null, Defaults));
        }

        [TestCase("MyGame.Player.Die()")]
        [TestCase("SystemShock.Player.Die()")]      // "System" must not match by plain string prefix
        [TestCase("UnityEngineExtras.Helper.Do()")]
        [TestCase("PlayerController.Update()")]     // no namespace
        [TestCase("<Start>d__3.MoveNext()")]
        public void Defaults_AppCodeIsInProject(string method)
        {
            Assert.IsTrue(InProjectResolver.IsInProject(method, null, Defaults));
        }

        [Test]
        public void NoMethod_IsInProjectWhenExcludeBased()
        {
            Assert.IsTrue(InProjectResolver.IsInProject(null, null, Defaults));
            Assert.IsTrue(InProjectResolver.IsInProject("", null, Defaults));
        }

        // ---- Include-based ----

        [Test]
        public void ProjectNamespaces_OnlyMatchingFramesAreInProject()
        {
            var include = new[] { "MyGame" };
            Assert.IsTrue(InProjectResolver.IsInProject("MyGame.Player.Die()", include, Defaults));
            Assert.IsTrue(InProjectResolver.IsInProject("MyGame:Start()", include, Defaults));
            Assert.IsFalse(InProjectResolver.IsInProject("OtherLib.Thing.Do()", include, Defaults));
            Assert.IsFalse(InProjectResolver.IsInProject("PlayerController.Update()", include, Defaults));
            Assert.IsFalse(InProjectResolver.IsInProject(null, include, Defaults));
        }

        [Test]
        public void ProjectNamespaces_EmptyOrBlankEntriesBehaveAsUnset()
        {
            Assert.IsTrue(InProjectResolver.IsInProject("OtherLib.Thing.Do()", new string[0], Defaults));
            Assert.IsTrue(InProjectResolver.IsInProject("OtherLib.Thing.Do()", new[] { "", null, "  " }, Defaults));
        }

        [Test]
        public void ProjectNamespaces_TrailingDotIsIgnored()
        {
            Assert.IsTrue(InProjectResolver.IsInProject("MyGame.Player.Die()", new[] { "MyGame." }, Defaults));
        }

        // ---- Precedence ----

        [Test]
        public void Precedence_LongestPrefixWins()
        {
            var include = new[] { "MyGame" };
            var exclude = new[] { "MyGame.ThirdParty" };
            Assert.IsTrue(InProjectResolver.IsInProject("MyGame.Player.Die()", include, exclude));
            Assert.IsFalse(InProjectResolver.IsInProject("MyGame.ThirdParty.Lib.Do()", include, exclude));

            // A more specific include overrides a default exclusion
            Assert.IsTrue(InProjectResolver.IsInProject("UnityEngine.MyFork.Do()", new[] { "UnityEngine.MyFork" }, Defaults));
        }

        [Test]
        public void Precedence_TieGoesToProjectNamespace()
        {
            Assert.IsTrue(InProjectResolver.IsInProject("Shared.Thing.Do()", new[] { "Shared" }, new[] { "Shared" }));
        }

        [Test]
        public void ExcludedNamespaces_NullDisablesExclusions()
        {
            Assert.IsTrue(InProjectResolver.IsInProject("UnityEngine.Debug:Log(Object)", null, null));
        }

        // ---- Applying to frames ----

        [Test]
        public void Apply_NeverOverridesExistingFalse()
        {
            var frame = Frame("MyGame.Player.Die()");
            frame.InProject = false; // e.g. iOS frame from a non-main image
            InProjectResolver.Apply(frame, null, Defaults);
            Assert.AreEqual(false, frame.InProject);
        }

        [Test]
        public void Apply_TrueIsNarrowedByExclusion()
        {
            // iOS main-image frames are pre-set to true and should become IsMainImage && !excluded
            var engine = Frame("UnityEngine.Debug:Log(Object)");
            engine.InProject = true;
            InProjectResolver.Apply(engine, null, Defaults);
            Assert.AreEqual(false, engine.InProject);

            var app = Frame("MyGame.Player.Die()");
            app.InProject = true;
            InProjectResolver.Apply(app, null, Defaults);
            Assert.AreEqual(true, app.InProject);
        }

        [Test]
        public void Apply_SetsValueWhenUnset()
        {
            var frame = Frame("DG.Tweening.Tween.DoGoto(Single)");
            Assert.IsNull(frame.InProject);
            InProjectResolver.Apply(frame, null, Defaults);
            Assert.AreEqual(false, frame.InProject);
            Assert.AreEqual(false, frame["inProject"]);
        }

        [Test]
        public void ApplyToErrors_UsesConfiguration()
        {
            var config = new Configuration("api-key");
            config.ProjectNamespaces = new[] { "MyGame" };
            var other = Frame("OtherLib.Thing.Do()");
            var app = Frame("MyGame.Player.Die()");
            InProjectResolver.ApplyToErrors(new[] { new Error("Exception", "boom", new[] { other, app }) }, config);
            Assert.AreEqual(false, other.InProject);
            Assert.AreEqual(true, app.InProject);
        }

        [Test]
        public void ApplyToErrors_SkipsAndroidJavaErrors()
        {
            var config = new Configuration("api-key");
            var javaFrame = Frame("java.lang.Thread.run");
            var javaError = new Error("java.lang.RuntimeException", "boom", new IStackframe[] { javaFrame },
                HandledState.ForUnhandledException(), true);
            var csFrame = Frame("UnityEngine.Debug:Log(Object)");
            var csError = new Error("Exception", "boom", new[] { csFrame });

            InProjectResolver.ApplyToErrors(new[] { javaError, csError, null }, config);

            Assert.IsNull(javaFrame.InProject);
            Assert.AreEqual(false, csFrame.InProject);
        }

        [Test]
        public void ApplyToErrors_SkipsUnprocessedAndroidJavaExceptionClass()
        {
            // Notify(exception, stacktrace) keeps the raw class but does not set IsAndroidJavaException
            var config = new Configuration("api-key");
            var javaFrame = Frame("java.lang.Thread.run");
            var javaError = new Error("AndroidJavaException", "java.lang.RuntimeException: boom", new[] { javaFrame });
            Assert.IsFalse(javaError.IsAndroidJavaException);

            InProjectResolver.ApplyToErrors(new[] { javaError }, config);

            Assert.IsNull(javaFrame.InProject);
        }

        [Test]
        public void ApplyToErrors_NullInputsDoNotThrow()
        {
            Assert.DoesNotThrow(() => InProjectResolver.ApplyToErrors(null, new Configuration("api-key")));
            Assert.DoesNotThrow(() => InProjectResolver.ApplyToErrors(new Error[0], null));
        }

        // ---- Configuration defaults ----

        [Test]
        public void Configuration_DefaultsToExcludeList()
        {
            var config = new Configuration("api-key");
            Assert.IsNull(config.ProjectNamespaces);
            CollectionAssert.Contains(config.ExcludedNamespaces, "UnityEngine");
            CollectionAssert.Contains(config.ExcludedNamespaces, "DG.Tweening");
        }

        [Test]
        public void Configuration_DefaultsAreACopy()
        {
            var config = new Configuration("api-key");
            config.ExcludedNamespaces[0] = "Changed";
            Assert.AreEqual("UnityEngine", Configuration.DefaultExcludedNamespaces[0]);
        }
    }
}
