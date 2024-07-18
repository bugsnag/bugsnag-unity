using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    public class Event : PayloadContainer, IEvent
    {

        internal Event(string context, Metadata metadata, AppWithState app, DeviceWithState device, User user, Error[] errors, HandledState handledState, List<Breadcrumb> breadcrumbs, Session session, string apiKey, OrderedDictionary featureFlags, LogType? logType = null)
        {
            ApiKey = apiKey;
            OriginalSeverity = handledState;
            _metadata = metadata;
            HandledState = handledState;
            LogType = logType;
            _appWithState = app;
            _deviceWithState = device;
            Context = context;
            _user = user;
            _errors = errors.ToList();
            Errors = new List<IError>();
            foreach (var error in _errors)
            {
                Errors.Add(error);
            }

            _breadcrumbs = breadcrumbs;
            var breadcrumbsList = new List<IBreadcrumb>();
            foreach (var crumb in _breadcrumbs)
            {
                breadcrumbsList.Add(crumb);
            }
            Breadcrumbs = new ReadOnlyCollection<IBreadcrumb>(breadcrumbsList);

            if (session != null)
            {
                if (handledState.Handled)
                {
                    session.Events.IncrementHandledCount();
                }
                else
                {
                    session.Events.IncrementUnhandledCount();
                }
                Session = session;
            }
            _featureFlags = featureFlags;
        }

        internal Event(Dictionary<string, object> serialisedPayload)
        {        
            ApiKey = serialisedPayload["apiKey"].ToString();

            var eventObject = (Dictionary<string, object>)serialisedPayload["event"];

            if (eventObject["unhandled"] != null)
            {
                Add("unhandled", eventObject["unhandled"]);
            }
            else
            {
                Add("unhandled", false);
            }
            
            if (eventObject["severity"] != null)
            {
                Add("severity", eventObject["severity"]);
            }
            if (eventObject["severityReason"] != null)
            {
                Add("severityReason", eventObject["severityReason"]);
            }
            _metadata = new Metadata();
            _metadata.MergeMetadata((Dictionary<string, object>)eventObject["metaData"]);

            _appWithState = new AppWithState((Dictionary<string, object>)eventObject["app"]);

            _deviceWithState = new DeviceWithState((Dictionary<string, object>)eventObject["device"]);

            _featureFlags = new OrderedDictionary();
            if (eventObject.ContainsKey("featureFlags"))
            {
                var flagsArray = (JsonArray)eventObject["featureFlags"];
                foreach (JsonObject flag in flagsArray)
                {
                    var featureFlag = new FeatureFlag(flag.GetDictionary());
                    _featureFlags[featureFlag.Name] = featureFlag.Variant;
                }
            }

            if (eventObject.ContainsKey("context"))
            {
                Context = eventObject["context"].ToString();
            }

            _user = new User();
            if (eventObject.ContainsKey("user"))
            {
                _user.Add((Dictionary<string, object>)eventObject["user"]);
            }

            if (eventObject.ContainsKey("breadcrumbs"))
            {
                _breadcrumbs = new List<Breadcrumb>();
                var crumbsArray = (JsonArray)eventObject["breadcrumbs"];
                foreach (JsonObject crumb in crumbsArray)
                {
                    _breadcrumbs.Add(new Breadcrumb(crumb.GetDictionary()));
                }
                var breadcrumbsList = new List<IBreadcrumb>();
                foreach (var crumb in _breadcrumbs)
                {
                    breadcrumbsList.Add(crumb);
                }
                Breadcrumbs = new ReadOnlyCollection<IBreadcrumb>(breadcrumbsList);
            }

            if (eventObject.ContainsKey("groupingHash"))
            {
                GroupingHash = eventObject["groupingHash"].ToString();
            }

            _errors = new List<Error>();
            var errorsArray = (JsonArray)eventObject["exceptions"];
            foreach (JsonObject error in errorsArray)
            {
                var newError = new Error(error.GetDictionary());
                _errors.Add(newError);

            }
            Errors = new List<IError>();
            foreach (var error in _errors)
            {
                Errors.Add(error);
            }

            if (eventObject.ContainsKey("session"))
            {
                Session = new Session();
                Session.Add((Dictionary<string, object>)eventObject["session"]);
            }

            if (eventObject.ContainsKey("projectPackages"))
            {
                var packagesList = new List<string>();
                var packagesArray = (JsonArray)eventObject["projectPackages"];
                foreach (var item in packagesArray)
                {
                    packagesList.Add(item.ToString());
                }
                _androidProjectPackages = packagesList.ToArray();
            }
        }

        internal void AddAndroidProjectPackagesToEvent(string[] packages)
        {
            _androidProjectPackages = packages;
        }

        private OrderedDictionary _featureFlags;

        HandledState _handledState;

        internal HandledState OriginalSeverity { get; }

        private string[] _androidProjectPackages;

        private Metadata _metadata { get; }

        public void AddMetadata(string section, string key, object value) => _metadata.AddMetadata(section,key,value);

        public void AddMetadata(string section, IDictionary<string, object> metadata) => _metadata.AddMetadata(section, metadata);

        public IDictionary<string, object> GetMetadata(string section) => _metadata.GetMetadata(section);

        public object GetMetadata(string section, string key) => _metadata.GetMetadata(section,key);

        public void ClearMetadata(string section) => _metadata.ClearMetadata(section);

        public void ClearMetadata(string section, string key) => _metadata.ClearMetadata(section, key);

        internal int PayloadVersion = 4;

        public string ApiKey { get; set; }

        private List<Breadcrumb> _breadcrumbs;

        public ReadOnlyCollection<IBreadcrumb> Breadcrumbs { get; }

        internal Session Session { get; }

        internal LogType? LogType { get; }

        public bool Unhandled {     
        get {
            var currentValue = Get("unhandled"); 
            if (currentValue == null) 
            {
                return false;
            }
            return (bool)currentValue;
        } 
        set => Add("unhandled",value); 
        }

        internal bool IsHandled
        {
            get
            {
                if (Get("unhandled") is bool unhandled)
                {
                    return !unhandled;
                }

                return false;
            }
        }

        public string Context { get; set; }

        private AppWithState _appWithState;

        public IAppWithState App => _appWithState;

        private DeviceWithState _deviceWithState;

        public IDeviceWithState Device => _deviceWithState;

        private List<Error> _errors;

        public List<IError> Errors { get; }

        public string GroupingHash { get; set; }
        
        public Severity Severity
        {
            set => HandledState = HandledState.ForCallbackSpecifiedSeverity(value, _handledState);
            get => _handledState.Severity;
        }



        private User _user;

        public IUser GetUser() => _user;

        public void SetUser(string id, string email, string name)
        {
            _user = new User(id, email, name );
        }

        internal bool IsAndroidJavaError()
        {
            foreach (var error in _errors)
            {
                if (error.IsAndroidJavaException)
                {
                    return true;
                }
            }
            return false;
        }

        HandledState HandledState
        {
            set
            {
                _handledState = value;
                foreach (var item in value)
                {
                    Payload[item.Key] = item.Value;
                }
            }
        }

        internal void RedactMetadata(Configuration config)
        {
            foreach (var section in _metadata.Payload)
            {
                var theSection = (IDictionary<string, object>)section.Value;
                foreach (var key in theSection.Keys.ToList())
                {
                    if (config.KeyIsRedacted(key))
                    {
                        theSection[key] = "[REDACTED]";
                    }
                }
            }
            foreach (var crumb in _breadcrumbs)
            {
                foreach (var key in crumb.Metadata.Keys.ToList())
                {
                    if (config.KeyIsRedacted(key))
                    {
                        crumb.Metadata[key] = "[REDACTED]";
                    }
                }
            }
        }

        public List<IThread> Threads => null;

        internal Dictionary<string, object> GetEventPayload()
        {
            Add("app", _appWithState.Payload);
            Add("device", _deviceWithState.Payload);
            Add("metaData", _metadata.Payload);
            Add("user", _user.Payload);
            Add("context", Context);
            Add("groupingHash", GroupingHash);
            Add("payloadVersion", PayloadVersion);
            Add("exceptions", _errors);
            if (_androidProjectPackages != null)
            {
                Add("projectPackages", _androidProjectPackages);
            }
            var breadcrumbPayloads = new List<Dictionary<string, object>>();
            foreach (var crumb in _breadcrumbs)
            {
                breadcrumbPayloads.Add(crumb.Payload);
            }
            Add("breadcrumbs", breadcrumbPayloads.ToArray());
            if (_featureFlags.Count > 0)
            {
                var featureFlagPayloads = new List<Dictionary<string, object>>();
                foreach (DictionaryEntry entry in _featureFlags)
                {
                    var flag = new FeatureFlag((string)entry.Key, (string)entry.Value);
                    featureFlagPayloads.Add(flag.Payload);
                }
                Add("featureFlags", featureFlagPayloads.ToArray());
            }
            if (Session != null)
            {
                Add("session", Session.Payload);
            }

            return Payload;
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            _featureFlags[name] = variant;
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
            _featureFlags.Remove(name);
        }

        public void ClearFeatureFlags()
        {
            _featureFlags.Clear();
        }

        public ReadOnlyCollection<FeatureFlag> FeatureFlags
        {
            get
            {
                List<FeatureFlag> list = new List<FeatureFlag>();
                foreach (DictionaryEntry entry in _featureFlags)
                {
                    list.Add(new FeatureFlag((string)entry.Key, (string)entry.Value));
                }
                return new ReadOnlyCollection<FeatureFlag>(list);
            }
        }
    }
}
