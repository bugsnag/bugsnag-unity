#if (UNITY_ANDROID && !UNITY_EDITOR) || BSG_ANDROID_DEV
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
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

                Frame.File = TrimFilenameIfRequired(mainImageFileName);
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

        private string TrimFilenameIfRequired(string filename)
        {
            // trim any excess characters from the filename that may appear after the `.so` extension
            const string extension = ".so";
            var lastSlashIndex = filename.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                return filename;
            }

            var extensionIndex = filename.LastIndexOf(extension, lastSlashIndex + 1);
            if (extensionIndex == -1)
            {
                return filename;
            }

            return filename.Substring(0, extensionIndex + extension.Length);
        }

#if ENABLE_IL2CPP && UNITY_2023_1_OR_NEWER

        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_gchandle_get_target(IntPtr gchandle);
        private static IntPtr Il2cpp_GHandle_Get_Target(IntPtr gchandle) => il2cpp_gchandle_get_target(gchandle);

#elif ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER

        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_gchandle_get_target(int gchandle);
        private static IntPtr Il2cpp_GHandle_Get_Target(IntPtr gchandle) => il2cpp_gchandle_get_target(gchandle.ToInt32());

#endif

#if ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER

        [DllImport("__Internal")]
        private static extern void il2cpp_free(IntPtr ptr);

        [DllImport("__Internal")]
        private static extern void il2cpp_native_stack_trace(IntPtr exc, out IntPtr addresses, out int numFrames, out IntPtr imageUUID, out IntPtr imageName);

#endif

        #nullable enable
        private static string? ExtractString(IntPtr pString)
        {
            return (pString == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(pString);
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

 #if ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER
             var hException = GCHandle.Alloc(exception);
             var pNativeAddresses = IntPtr.Zero;
             var pImageUuid = IntPtr.Zero;
             var pImageName = IntPtr.Zero;
             try
             {
                 if (hException == null)
                 {
                     return notFound;
                 }

                 var pException = Il2cpp_GHandle_Get_Target(GCHandle.ToIntPtr(hException));

                 if (pException == IntPtr.Zero)
                 {
                     return notFound;
                 }

                 var frameCount = 0;
                 string? mainImageFileName = null;
                 string? mainImageUuid = null;

                 il2cpp_native_stack_trace(pException, out pNativeAddresses, out frameCount, out pImageUuid, out pImageName);
                 if (pNativeAddresses == IntPtr.Zero)
                 {
                     return notFound;
                 }

                 mainImageFileName = ExtractString(pImageName);
                 mainImageUuid = ExtractString(pImageUuid);

                 var nativeAddresses = new IntPtr[frameCount];
                 Marshal.Copy(pNativeAddresses, nativeAddresses, 0, frameCount);

                 return ToStackFrames(exception, nativeAddresses, mainImageFileName, mainImageUuid);
             }
             finally
             {
                 if (pImageUuid != IntPtr.Zero)
                 {
                     il2cpp_free(pImageUuid);
                 }
                 if (pImageName != IntPtr.Zero)
                 {
                     il2cpp_free(pImageName);
                 }
                 if (pNativeAddresses != IntPtr.Zero)
                 {
                     il2cpp_free(pNativeAddresses);
                 }
                 if (hException != null)
                 {
                     hException.Free();
                 }
             }
 #else
             return notFound;
 #endif
        }
    }

}
#endif
