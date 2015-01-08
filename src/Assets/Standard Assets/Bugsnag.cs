using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
#endif

public class Bugsnag : MonoBehaviour {
    public class NativeBugsnag {
        #if UNITY_IPHONE && !UNITY_EDITOR
            [DllImport ("__Internal")]
            public static extern void SetUserId(string userId);
        
            [DllImport ("__Internal")]
            public static extern void SetContext(string context);
        
            [DllImport ("__Internal")]
            public static extern void SetReleaseStage(string releaseStage);
        
            [DllImport ("__Internal")]
            public static extern void Notify(string errorClass, string errorMessage, string stackTrace);
                
            [DllImport ("__Internal")]
            public static extern void Register(string apiKey);
                
            [DllImport ("__Internal")]
            public static extern void SetUseSSL(bool useSSL);
            
            [DllImport ("__Internal")]
            public static extern void SetAutoNotify(bool autoNotify);
                
            [DllImport ("__Internal")]
            public static extern void AddToTab(string tabName, string attributeName, string attributeValue);
                
            [DllImport ("__Internal")]
            public static extern void ClearTab(string tabName);
        #elif UNITY_ANDROID && !UNITY_EDITOR
            [DllImport ("bugsnag")]
            public static extern void SetUserId(string userId);
        
            [DllImport ("bugsnag")]
            public static extern void SetContext(string context);
        
            [DllImport ("bugsnag")]
            public static extern void SetReleaseStage(string releaseStage);
        
            [DllImport ("bugsnag")]
            public static extern void Notify(string errorClass, string errorMessage, string stackTrace);
                
            [DllImport ("bugsnag")]
            public static extern void Register(string apiKey);
                
            [DllImport ("bugsnag")]
            public static extern void SetUseSSL(bool useSSL);
            
            [DllImport ("bugsnag")]
            public static extern void SetAutoNotify(bool autoNotify);
                
            [DllImport ("bugsnag")]
            public static extern void AddToTab(string tabName, string attributeName, string attributeValue);
                
            [DllImport ("bugsnag")]
            public static extern void ClearTab(string tabName);
        #else
            public static void SetUserId(string userId) {}
            public static void SetContext(string context) {}
            public static void SetReleaseStage(string releaseStage) {}
            public static void Notify(string errorClass, string errorMessage, string stackTrace) {}
            public static void Register(string apiKey) {}
            public static void SetUseSSL(bool useSSL) {}
            public static void SetAutoNotify(bool autoNotify) {}
            public static void AddToTab(string tabName, string attributeName, string attributeValue) {}
            public static void ClearTab(string tabName) {}
        #endif
    }
        
    // We dont use the LogType enum in Unity as the numerical order doesnt suit our purposes
    public enum LogSeverity {
        Log,
        Warning,
        Assert,
        Error,
        Exception
    }

    public string BugsnagApiKey = "";
    public bool AutoNotify = true;
    public bool UseSSL = true;
    public LogSeverity NotifyLevel = LogSeverity.Exception;
    
    public string UserId {
        set {
            NativeBugsnag.SetUserId(value);
        }
    }
        
    public string ReleaseStage {
        set {
            NativeBugsnag.SetReleaseStage(value);
        }
    }
        
    public string Context { 
        set {
            NativeBugsnag.SetContext(value);
        }
    }
        
    void Awake() {
        DontDestroyOnLoad(this);
        NativeBugsnag.Register(BugsnagApiKey);
        
        if(Debug.isDebugBuild) {
            ReleaseStage = "development";
        } else {
            ReleaseStage = "production";
        }
        
        NativeBugsnag.SetContext(Application.loadedLevelName);
        NativeBugsnag.SetUseSSL(UseSSL);
        NativeBugsnag.SetAutoNotify(AutoNotify);
    }
    
    void OnEnable () {
        Application.RegisterLogCallback(HandleLog);
    }
    
    void OnDisable () {
        // Remove callback when object goes out of scope
        Application.RegisterLogCallback(null);
    }
        
    void OnLevelWasLoaded(int level) {
        NativeBugsnag.SetContext(Application.loadedLevelName);
    }
    
    void HandleLog (string logString, string stackTrace, LogType type) {
        LogSeverity severity = LogSeverity.Exception;
        
        switch (type) {
        case LogType.Assert:
            severity = LogSeverity.Assert;
            break;
        case LogType.Error:
            severity = LogSeverity.Error;
            break;
        case LogType.Exception:
            severity = LogSeverity.Exception;
            break;
        case LogType.Log:
            severity = LogSeverity.Log;
            break;
        case LogType.Warning:
            severity = LogSeverity.Warning;
            break;
        default:
            break;
        }

        if(severity >= NotifyLevel && AutoNotify) {
            string errorClass, errorMessage = null;
            
            Regex exceptionRegEx = new Regex(@"^(?<errorClass>\S+):\s*(?<message>.*)");
            Match match = exceptionRegEx.Match(logString);

            if(match.Success) {
                errorClass = match.Groups["errorClass"].Value;
                errorMessage = match.Groups["message"].Value.Trim();
            } else {
                errorClass = logString;
            }
                
            NativeBugsnag.Notify(errorClass, errorMessage, stackTrace);
        }
    }

    public static void Notify(Exception e) {
        if(e != null) {
            if(e.StackTrace == null) {
                try {
                    throw e;
                } catch(Exception ex) {
                    e = ex;
                }
            }
            NativeBugsnag.Notify(e.GetType().ToString(), e.Message, e.StackTrace);
        }
    }
        
    public static void AddToTab(string tabName, string attributeName, string attributeValue) {
        NativeBugsnag.AddToTab(tabName, attributeName, attributeValue);
    }
        
    public static void ClearTab(string tabName) {
        NativeBugsnag.ClearTab(tabName);
    }

#if UNITY_EDITOR
    public class BugsnagBuilder : MonoBehaviour {
        
        // We need to enable ARC on all of the Bugsnag files, this is a fairly simple find and replace:
        //
        // D8A1C700dE80637F000160D4 /* Bugsnag.m in Sources */ = {isa = PBXBuildFile; fileRef = D8A1C700dE80637F100160D4 /* Bugsnag.m */; };
        // D8A1C700dE80637F000160D4 /* Bugsnag.m in Sources */ = {isa = PBXBuildFile; fileRef = D8A1C700dE80637F100160D4 /* Bugsnag.m */; settings = {COMPILER_FLAGS = "-fobjc-arc"; }; };
        static string[] BUGSNAG_FILES = {
            "Bugsnag.m",
            "BugsnagEvent.m",
            "BugsnagMetaData.m",
            "BugsnagNotifier.m",
            "BugsnagUnity.mm",
            "NSDictionary-BSJSON.m",
            "NSMutableDictionary-BSMerge.m",
            "NSNumber-BSDuration.m",
            "NSNumber-BSFileSizes.m",
            "Reachability.m",
            "UIDevice-BSStats.m",
            "UIViewController-BSVisibility.m"
        };
        static Regex _matcher = null;
        
        // Thanks to https://gist.github.com/tenpn/f8da1b7df7352a1d50ff for inspiration for this code.
        [PostProcessBuild(1400)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iPhone) {
                return;
            }
            
            Regex fileMatcher = getFileMatcher ();
            
            var xcodeProjectPath = Path.Combine (path, "Unity-iPhone.xcodeproj");
            var pbxPath = Path.Combine (xcodeProjectPath, "project.pbxproj");
            
            var sb = new StringBuilder ();
            
            var xcodeProjectLines = File.ReadAllLines (pbxPath);
            
            foreach (var line in xcodeProjectLines) {
                // Enable ARC where required
                if (fileMatcher.IsMatch (line)) {
                    var index = line.LastIndexOf("}");
                    var newLine = line.Substring (0, index) + "settings = {COMPILER_FLAGS = \"-fobjc-arc\"; }; " + line.Substring(index);
                    
                    sb.AppendLine(newLine);
                    
                    // Enable objective C exceptions
                } else if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS")) {
                    var newLine = line.Replace("NO", "YES");
                    sb.AppendLine(newLine);
                    
                } else {
                    sb.AppendLine(line);
                }
                
            }
            
            File.WriteAllText(pbxPath, sb.ToString());
        }
        
        // Regex to try and find lines identifying the bugsnag source files in the Xcode project.
        private static Regex getFileMatcher() {
            
            if (_matcher == null) {
                var sb = new StringBuilder();
                sb.Append ("( ");
                
                for (int i = 0; i < BUGSNAG_FILES.Length; i++) {
                    if (i > 0) {
                        sb.Append (" | ");
                    }
                    sb.Append(Regex.Escape(BUGSNAG_FILES[i]));
                }
                
                sb.Append (" )");
                
                _matcher = new Regex("isa = PBXBuildFile.*" + sb.ToString() + "");
            }
            return _matcher;
        }
    }
#endif
}
