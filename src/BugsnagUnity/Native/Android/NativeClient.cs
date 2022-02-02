using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public Configuration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        public IDelivery Delivery { get; }

        private NativeInterface NativeInterface;

        public NativeClient(Configuration configuration)
        {
            NativeInterface = new NativeInterface(configuration);
            Configuration = configuration;
            Delivery = new Delivery();
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
            foreach (var pair in data)
            {
                NativeInterface.AddMetadata(section,pair.Key,pair.Value.ToString());
            }
        }

        public List<FeatureFlag> GetFeatureFlags()
        {
            throw new System.NotImplementedException();
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            throw new System.NotImplementedException();
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            throw new System.NotImplementedException();
        }

        public void ClearFeatureFlag(string name)
        {
            throw new System.NotImplementedException();
        }

        public void ClearFeatureFlags()
        {
            throw new System.NotImplementedException();
        }
    }

}
