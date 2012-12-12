using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using MiniJSON;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Bugsnag {
	public static class ExtensionMethods
	{
	    // Deep clone
	    public static T DeepClone<T>(this T a)
	    {
	        using (MemoryStream stream = new MemoryStream())
	        {
	            BinaryFormatter formatter = new BinaryFormatter();
	            formatter.Serialize(stream, a);
	            stream.Position = 0;
	            return (T) formatter.Deserialize(stream);
	        }
	    }
	}
	
	[Serializable()]
	class BugsnagMetaData : ISerializable {
		public BugsnagMetaData() {
			Data = new Dictionary<string, object>();
		}
		
		public BugsnagMetaData(SerializationInfo info, StreamingContext ctxt) {
		    Data = (Dictionary<string,object>)info.GetValue("Data", typeof(Dictionary<string,object>));
		}
		        
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
		    info.AddValue("Data", Data);
		}
		
		public Dictionary<string,object> Data {get; set;}
		
		public Dictionary<string,object> GetTab(string name) {
			if(!Data.ContainsKey(name)) {
				Data.Add(name, new Dictionary<string, object>());
			}
			return (Dictionary<string,object>)Data[name];
		}
		
		public void ClearTab(string name) {
			if(Data.ContainsKey(name)) {
				Data.Remove(name);
			}
		}
		
		public void AddAttributeToTab(string tab, string attribute, object attributeValue) {
			Dictionary<string,object> tabDict = GetTab(tab);
			tabDict.Add(attribute, attributeValue);
		}
	}
	
	public class Bugsnag : MonoBehaviour {
		public bool EnableSSL { get; set; }
		public string ApiKey { get; set; }
		public string UserId {
			get {
				if(userId == null) {
					try {
						userId = File.ReadAllText(userIdFilename);
					} catch(Exception e) {
						UserId = SystemInfo.deviceUniqueIdentifier;
					}
				}
				return userId;
			}
			
			set {
				if(userId != value) {
					userId = value;
					using (StreamWriter stream = File.CreateText(userIdFilename)) {
						stream.WriteLine(userId);
					}
				}
			}
		}
		
		public string ReleaseStage {
			get {
				if(releaseStage == null) {
					if(Debug.isDebugBuild) {
						releaseStage = "development";
					} else {
						releaseStage = "production";
					}
				}
				return releaseStage;
			}
			
			set {
				releaseStage = value;
			}
		}
		
		public string Context { 
			get {
				if(context != null) return context;
				return Application.loadedLevelName;
			}
			
			set {
				context = value;
			}
		}
		
		private BugsnagMetaData MetaData = new BugsnagMetaData();
		private string releaseStage;
		private string userId;
		private string context;
		
		private string persistantDataPath;
		private string userIdFilename;
		private string notifyEndpoint = "192.168.1.131:8000";
		private string[] fileNames;
		private string NotifyURL {
			get {
				if(EnableSSL) {
					return "https://" + notifyEndpoint;
				} else {
					return "http://" + notifyEndpoint;
				}
			}
		}
		
		private Dictionary<string, string> NotifierInfo {
			get {
				Dictionary<string, string> returnValue = new Dictionary<string, string>();
				returnValue.Add("name", "Bugsnag Unity");
				returnValue.Add("version", "1.0.0");
				returnValue.Add("url", "https://github.com/bugsnag/bugsnag-unity");
				return returnValue;
			}
		}
		
		private string EventDirectory {
			get {
				return Path.Combine(persistantDataPath, "Bugsnag");
			}
		}
		
		void Awake() {
			ApiKey = "b306fbaec7befc68398cecd25d1c65a7";
			persistantDataPath = Application.persistentDataPath;
			
			Directory.CreateDirectory(EventDirectory);
			userIdFilename = Path.Combine(Application.persistentDataPath, "bugsnag_user_id");
		}
	
		void OnEnable () {
	    	Application.RegisterLogCallback(HandleLog);
			SendCachedEvents();
		}
	
		void OnDisable () {
	    	// Remove callback when object goes out of scope
		    Application.RegisterLogCallback(null);
		}
	
		void HandleLog (string logString, string stackTrace, LogType type) {
			if(type == LogType.Exception) {
				Dictionary<string,object> eventDict = GenerateEvent(logString, stackTrace);
				
				// Should be async
				using(StreamWriter stream = File.CreateText(GenerateNewEventFilename())) {
					stream.WriteLine(Json.Serialize(eventDict));
				}
				SendCachedEvents();
			}
		}
		
		private string GenerateNewEventFilename() {
			DateTime centuryBegin = new DateTime(2012, 1, 1);
			DateTime currentDate = DateTime.Now;
			long elapsedTicks = currentDate.Ticks - centuryBegin.Ticks;
			return Path.Combine(EventDirectory, elapsedTicks.ToString());
		}
		
		private Dictionary<string,object> GenerateEvent(string logString, string stackTrace) {
			Dictionary<string,object> eventDict = new Dictionary<string, object>();
			eventDict.Add("userId", UserId);
			eventDict.Add("context", Context);
			eventDict.Add("releaseStage",ReleaseStage);
			eventDict.Add("exceptions", GenerateExceptionList(logString, stackTrace));
			
			BugsnagMetaData eventMetaData = MetaData.DeepClone();
			eventDict.Add("metaData", eventMetaData.Data);
			
			eventDict.Add("osVersion", SystemInfo.operatingSystem);
			
			eventMetaData.AddAttributeToTab("device", "supportsAccelerometer", SystemInfo.supportsAccelerometer);
			eventMetaData.AddAttributeToTab("device", "supportsGyroscope", SystemInfo.supportsGyroscope);
			eventMetaData.AddAttributeToTab("device", "supportsLocationService", SystemInfo.supportsLocationService);
			eventMetaData.AddAttributeToTab("device", "supportsVibration", SystemInfo.supportsVibration);
			
			eventMetaData.AddAttributeToTab("device", "systemMemorySize", SystemInfo.systemMemorySize);
			eventMetaData.AddAttributeToTab("device", "graphicsMemorySize", SystemInfo.graphicsMemorySize);
			
			//eventMetaData.AddAttributeToTab("application", "packageIdentifier", PlayerSettings.bundleIdentifier);
			
#if UNITY_IPHONE || UNITY_ANDROID
			//eventDict.Add("appVersion", PlayerSettings.bundleVersion);
			//eventMetaData.AddAttributeToTab("application", "applicationVersion", PlayerSettings.bundleVersion);
#endif

#if UNITY_IPHONE
			eventMetaData.AddAttributeToTab("device", "deviceType", "iphone");
			eventMetaData.AddAttributeToTab("device", "model", iPhone.generation.ToString());
#endif
			
#if UNITY_ANDROID
			eventMetaData.AddAttributeToTab("device", "deviceType", "android");
			eventMetaData.AddAttributeToTab("device", "model", SystemInfo.deviceModel);
#endif
			
			return eventDict;
		}
		
		private List<Dictionary<string,object>> GenerateStackTraceList(string stackTrace) {
			List<Dictionary<string,object>> stackTraceList = new List<Dictionary<string, object>>();
			
			string[] stackTraceSplit = stackTrace.Trim().Split('\n');
			Regex stackTraceRegEx = new Regex(@"^(?<method>\S+).+?(?<file>\S+):(?<line>\d*)");
			foreach(string stackLine in stackTraceSplit) {
				Match match = stackTraceRegEx.Match(stackLine);
				string method, lineNumber, file;
				if(match.Success) {
					method = match.Groups["method"].Value;
					lineNumber = match.Groups["line"].Value;
					file = match.Groups["file"].Value;
				} else {
					// This may well break grouping!
					lineNumber = "0";
					file = "unknown";
					method = stackLine;
				}
				
				Dictionary<string,object> stackFrame = new Dictionary<string, object>();
				try {
					stackFrame.Add("lineNumber", Int64.Parse(lineNumber));
				} catch (Exception e) {
					stackFrame.Add("lineNumber", 0);
				}
				stackFrame.Add("method", method);
				stackFrame.Add ("file", file);
				
				stackTraceList.Add(stackFrame);
			}
			return stackTraceList;
		}
		
		private List<Dictionary<string,object>> GenerateExceptionList(string logString, string stackTrace) {
			string errorClass, errorMessage = null;
			
			Regex exceptionRegEx = new Regex(@"^(?<errorClass>\S+):\s*(?<message>.*)");
			Match match = exceptionRegEx.Match(logString);
			
			if(match.Success) {
				errorClass = match.Groups["errorClass"].Value;
				errorMessage = match.Groups["message"].Value.Trim();
			} else {
				errorClass = logString;
			}
			
			List<Dictionary<string,object>> exceptions = new List<Dictionary<string, object>>();
			Dictionary<string,object> exception = new Dictionary<string, object>();
			exceptions.Add(exception);
			
			exception.Add("errorClass", errorClass);
			exception.Add("stacktrace", GenerateStackTraceList(stackTrace));
			
			if(errorMessage != null && errorMessage.Length != 0) {
				exception.Add("message", errorMessage);
			}
			
			return exceptions;
		}
		
		private List<Dictionary<string,object>> OutstandingEvents() {
			try {
				fileNames = Directory.GetFiles(EventDirectory);
				List<Dictionary<string,object>> events = new List<Dictionary<string,object>>();
				foreach(string fileName in fileNames) {
					events.Add((Dictionary<string,object>)Json.Deserialize(File.ReadAllText(fileName)));
				}
				return events;
			} catch(DirectoryNotFoundException e) {
				return null;
			}
		}
		
		private void SendCachedEvents() {
			// Should be async
			List<Dictionary<string,object>> events = OutstandingEvents();
			if(events != null && events.Count != 0) {
				Dictionary<string, object> payloadDict = new Dictionary<string, object>();
				payloadDict.Add("apiKey", ApiKey);
				payloadDict.Add("notifier", NotifierInfo);
				payloadDict.Add("events", events);
				
				string jsonString = Json.Serialize(payloadDict);
				
				byte[] payload = Encoding.UTF8.GetBytes(jsonString);
				
				Hashtable headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				headers.Add("Content-Length", jsonString.Length);
				
				WWW request = new WWW(NotifyURL, payload, headers);
				
				StartCoroutine(WaitForResponse(request));
			}
		}
		
		private IEnumerator WaitForResponse(WWW request) {
			yield return request;
			
			if(request.error == null) {
				Debug.Log ("Request OK!: " + request.data);
				foreach(string fileName in fileNames) {
					File.Delete(fileName);
				}
			} else {
				Debug.Log ("Request Error!: " + request.error);
			}
		}
	}
}