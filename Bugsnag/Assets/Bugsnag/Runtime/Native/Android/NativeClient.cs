#if (UNITY_ANDROID && !UNITY_EDITOR) || BSG_ANDROID_DEV
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using BugsnagUnity;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {

        private const string ANDROID_JAVA_EXCEPTION_CLASS = "AndroidJavaException";

        public Configuration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        private NativeInterface NativeInterface;

        public NativeClient(Configuration configuration)
        {
            NativeInterface = new NativeInterface(configuration);
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(NativeInterface);
            if (configuration.AutoTrackSessions)
            {
                ResumeSession();
            }
        }

        public void PopulateApp(App app)
        {
            app.Add(NativeInterface.GetApp());
        }

        public void PopulateAppWithState(AppWithState app)
        {
            PopulateApp(app);
        }

        public void PopulateDevice(Device device)
        {
            Dictionary<string, object> nativeDeviceData = NativeInterface.GetDevice();
            Dictionary<string, object> nativeVersions = (Dictionary<string, object>)nativeDeviceData.Get("runtimeVersions");

            nativeDeviceData.Remove("runtimeVersions"); // don't overwrite the unity version values
            device.Add(nativeDeviceData);
            MergeDictionaries(device.RuntimeVersions, nativeVersions); // merge the native version values
        }

        public void PopulateDeviceWithState(DeviceWithState device)
        {
            PopulateDevice(device);
        }

        public void PopulateUser(User user)
        {
            foreach (var entry in NativeInterface.GetUser())
            {
                user.Payload.AddToPayload(entry.Key, entry.Value.ToString());
            }
        }

        private void MergeDictionaries(IDictionary<string, object> dest, IDictionary<string, object> another)
        {
            foreach (var entry in another)
            {
                dest.AddToPayload(entry.Key, entry.Value);
            }
        }

        public void SetUser(User user)
        {
            NativeInterface.SetUser(user);
        }

        public void SetContext(string context)
        {
            NativeInterface.SetContext(context);
        }

        public void StartSession()
        {
            NativeInterface.StartSession();
        }

        public void PauseSession()
        {
            NativeInterface.PauseSession();
        }

        public bool ResumeSession()
        {
            return NativeInterface.ResumeSession();
        }

        public void UpdateSession(Session session)
        {
            NativeInterface.UpdateSession(session);
        }

        public Session GetCurrentSession()
        {
            return NativeInterface.GetCurrentSession();
        }

        public void MarkLaunchCompleted()
        {
            NativeInterface.MarkLaunchCompleted();
        }

        public LastRunInfo GetLastRunInfo()
        {
            return NativeInterface.GetlastRunInfo();
        }

        public void ClearNativeMetadata(string section)
        {
            NativeInterface.ClearMetadata(section);
        }

        public void ClearNativeMetadata(string section, string key)
        {
            NativeInterface.ClearMetadata(section,key);
        }

        public IDictionary<string, object> GetNativeMetadata()
        {
            return NativeInterface.GetMetadata();
        }

        public void AddNativeMetadata(string section, IDictionary<string, object> data)
        {
            NativeInterface.AddMetadata(section,data);
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            NativeInterface.AddFeatureFlag(name, variant);
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            foreach (var flag in featureFlags)
            {
                AddFeatureFlag(flag.Name, flag.Variant);
            }
        }

        public void ClearFeatureFlag(string name)
        {
            NativeInterface.ClearFeatureFlag(name);
        }

        public void ClearFeatureFlags()
        {
            NativeInterface.ClearFeatureFlags();
        }

        public bool ShouldAttemptDelivery()
        {
            return true;
        }

        public void RegisterForOnSessionCallbacks()
        {
            NativeInterface.RegisterForOnSessionCallbacks();
        }

        private StackTraceLine[] ToStackFrames(System.Exception exception, IntPtr[] nativeAddresses, String mainImageFileName, String mainImageUuid)
        {
            StackTraceLine[] lines;
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                lines = new PayloadStackTrace(exception.StackTrace).StackTraceLines;
            }
            else
            {
                return new StackTraceLine[0];
            }

            var StackTraceLength = nativeAddresses.Length;
            StackTraceLine[] Trace = new StackTraceLine[StackTraceLength];

            for (int i = 0; i < StackTraceLength; i++)
            {
                StackTraceLine Frame = new StackTraceLine();

                Frame.File = mainImageFileName;
                Frame.Method = lines[i].Method;
                Frame.FrameAddress = string.Format("0x{0:X}", nativeAddresses[i].ToInt64());
                Frame.LoadAddress = "0x0";
                Frame.CodeIdentifier = mainImageUuid;
                Frame.Type = "c";

                // we mark every stack frame as "PC" so that the addresses are not adjusted
                Frame.IsPc = true;

                Trace[i] = Frame;
            }

            return Trace;
        }

        public StackTraceLine[] ToStackFrames(System.Exception exception)
        {
            var notFound = new StackTraceLine[0];
            if (exception == null)
            {
                return notFound;
            }

            var errorClass = exception.GetType().Name;
            if (errorClass == ANDROID_JAVA_EXCEPTION_CLASS)
            {
                return notFound;
            }

            return Il2cppUtils.ToStackFrames(
                exception,
                (exception, nativeAddresses, mainImageFileName, mainImageUuid) =>
                    ToStackFrames(exception, nativeAddresses, mainImageFileName, mainImageUuid)
            );
        }
    }

}
#endif
