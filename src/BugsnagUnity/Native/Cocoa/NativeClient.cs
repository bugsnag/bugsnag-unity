﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public IConfiguration Configuration { get; }
        public IBreadcrumbs Breadcrumbs { get; }
        public IDelivery Delivery { get; }

        IntPtr NativeConfiguration { get; }

        public NativeClient(IConfiguration configuration)
        {
            Configuration = configuration;
            NativeConfiguration = CreateNativeConfig(configuration);
            NativeCode.bugsnag_startBugsnagWithConfiguration(NativeConfiguration, NotifierInfo.NotifierVersion);
            Delivery = new Delivery();
            Breadcrumbs = new Breadcrumbs();
        }

        /**
         * Transforms an IConfiguration C# object into a Cocoa Configuration object.
         */
        IntPtr CreateNativeConfig(IConfiguration config)
        {
            IntPtr obj = NativeCode.bugsnag_createConfiguration(config.ApiKey);
            NativeCode.bugsnag_setAppHangs(obj, true);
            NativeCode.bugsnag_setAutoNotifyConfig(obj, config.AutoNotify);
            NativeCode.bugsnag_setReleaseStage(obj, config.ReleaseStage);
            NativeCode.bugsnag_setAppVersion(obj, config.AppVersion);
            NativeCode.bugsnag_setNotifyUrl(obj, config.Endpoint.ToString());
            NativeCode.bugsnag_setMaxBreadcrumbs(obj, config.MaximumBreadcrumbs);
            SetEnabledBreadcrumbTypes(obj,config);
            if (config.Context != null)
            {
                NativeCode.bugsnag_setContextConfig(obj, config.Context);
            }
            var releaseStages = config.NotifyReleaseStages;
            if (releaseStages != null)
            {
                NativeCode.bugsnag_setNotifyReleaseStages(obj, releaseStages, releaseStages.Length);
            }
            return obj;
        }

        private void SetEnabledBreadcrumbTypes(IntPtr obj, IConfiguration config)
        {
            if (config.EnabledBreadcrumbTypes == null)
            {
                NativeCode.bugsnag_setEnabledBreadcrumbTypes(obj, null, 0);
                return;
            }
            var enabledTypes = new List<string>();
            foreach (var enabledType in config.EnabledBreadcrumbTypes)
            {
                var typeName = Enum.GetName(typeof(BreadcrumbType), enabledType);
                enabledTypes.Add(typeName);
            }
            NativeCode.bugsnag_setEnabledBreadcrumbTypes(obj, enabledTypes.ToArray(),enabledTypes.Count);
        }

        public void PopulateApp(App app)
        {
            GCHandle handle = GCHandle.Alloc(app);

            try
            {
                NativeCode.bugsnag_retrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
            }
            finally
            {
                if (handle != null)
                {
                    handle.Free();
                }
            }
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
        static void PopulateAppData(IntPtr instance, string key, string value)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is App app)
            {
                app.AddToPayload(key, value);
            }
        }

        public void PopulateDevice(Device device)
        {
            var handle = GCHandle.Alloc(device);

            try
            {
                NativeCode.bugsnag_retrieveDeviceData(GCHandle.ToIntPtr(handle), PopulateDeviceData);
            }
            finally
            {
                handle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
        static void PopulateDeviceData(IntPtr instance, string key, string value)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is Device device)
            {
                switch (key)
                {
                    case "jailbroken":
                        switch (value)
                        {
                            case "0":
                                device.AddToPayload(key, false);
                                break;
                            case "1":
                                device.AddToPayload(key, true);
                                break;
                            default:
                                device.AddToPayload(key, value);
                                break;
                        }
                        break;
                    case "osBuild": // add to nested runtimeVersions dictionary
                        Dictionary<string, object> runtimeVersions = (Dictionary<string, object>)device.Get("runtimeVersions");
                        runtimeVersions.AddToPayload(key, value);
                        break;
                    default:
                        device.AddToPayload(key, value);
                        break;
                }
            }
        }

        public void PopulateUser(User user)
        {
            var nativeUser = new NativeUser();

            NativeCode.bugsnag_populateUser(ref nativeUser);

            user.Id = Marshal.PtrToStringAuto(nativeUser.Id);
        }

        public void SetMetadata(string tab, Dictionary<string, string> unityMetadata)
        {
            var index = 0;
            var count = 0;
            if (unityMetadata != null)
            {
                var metadata = new string[unityMetadata.Count * 2];

                foreach (var data in unityMetadata)
                {
                    if (data.Key != null)
                    {
                        metadata[index] = data.Key;
                        metadata[index + 1] = data.Value;
                        count += 2;
                    }
                    index += 2;
                }
                NativeCode.bugsnag_setMetadata(NativeConfiguration, tab, metadata, count);
            }
            else
            {
                NativeCode.bugsnag_removeMetadata(NativeConfiguration, tab);
            }
        }

        public void PopulateMetadata(Metadata metadata)
        {
            var handle = GCHandle.Alloc(metadata);
            try
            {
                NativeCode.bugsnag_retrieveMetaData(GCHandle.ToIntPtr(handle), PopulateMetaDataData);
            }
            finally
            {
                handle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(NativeCode.MetadataInformation))]
        static void PopulateMetaDataData(IntPtr instance, string tab, string[] keys, int keysSize, string[] values, int valuesSize)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is Metadata metadata)
            {
                var metadataObject = new Dictionary<string, string>();
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    var value = values[i];
                    if (key.Equals("simulator"))
                    {
                        value = value.Equals("0") || value.Equals("false") ? "false" : "true";
                    }
                    metadataObject.Add(key, value);
                }
                metadata.AddToPayload(tab, metadataObject);
            }
        }

        public void SetSession(Session session)
        {
            if (session == null)
            {
                // Clear session
                NativeCode.bugsnag_registerSession(null, 0, 0, 0);
            }
            else
            {
                // The ancient version of the runtime used doesn't have an equivalent to GetUnixTime()
                var startedAt = Convert.ToInt64((session.StartedAt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                NativeCode.bugsnag_registerSession(session.Id.ToString(), startedAt, session.UnhandledCount(), session.HandledCount());
            }
        }

        public void SetUser(User user)
        {
            NativeCode.bugsnag_setUser(user.Id, user.Name, user.Email);
        }

        public void SetContext(string context)
        {
            NativeCode.bugsnag_setContext(NativeConfiguration, context);
        }

        public void SetAutoNotify(bool autoNotify)
        {
            NativeCode.bugsnag_setAutoNotify(autoNotify);
        }

        public void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
        }
    }
}
