using BugsnagUnity.Payload;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public IConfiguration Configuration { get; }

        public IBreadcrumbs Breadcrumbs { get; }

        public IDelivery Delivery { get; }

        private Dictionary<string, object> _editorMetadata = new Dictionary<string, object>();

        public NativeClient(IConfiguration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
            Delivery = new Delivery();
        }

        public void PopulateApp(App app)
        {
            app.AddToPayload("type",GetAppType());
        }

        public void PopulateDevice(Device device)
        {
        }

        public void PopulateMetadata(Metadata metadata)
        {
            MergeDictionaries(metadata, _editorMetadata);
        }
        private void MergeDictionaries(Dictionary<string, object> dest, Dictionary<string, object> another)
        {
            foreach (var entry in another)
            {
                dest.AddToPayload(entry.Key, entry.Value);
            }
        }

        public void PopulateUser(User user)
        {
        }

        public void SetMetadata(string tab, Dictionary<string, string> metadata)
        {
            _editorMetadata[tab] = metadata;
        }

        public void SetSession(Session session)
        {
        }

        public void SetUser(User user)
        {
        }
        public void SetContext(string context)
        {
        }
        public void SetAutoNotify(bool autoNotify)
        {
        }

        public void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
        }

        private string GetAppType()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";
                case RuntimePlatform.WindowsPlayer:      
                case RuntimePlatform.WindowsEditor:
                    return "Windows";               
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "Linux";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                default:
                    return string.Empty;
            }
        }
    }
}
