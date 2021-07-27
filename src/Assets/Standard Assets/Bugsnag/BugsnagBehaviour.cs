using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BugsnagUnity.Payload;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace BugsnagUnity
{
    public class BugsnagBehaviour : MonoBehaviour
    {

        private class LabelOverride : PropertyAttribute
        {
            public string label;
            public LabelOverride(string label)
            {
                this.label = label;
            }

#if UNITY_EDITOR
            [CustomPropertyDrawer(typeof(LabelOverride))]
            public class ThisPropertyDrawer : PropertyDrawer
            {
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    try
                    {
                        var propertyAttribute = this.attribute as LabelOverride;
                        label.text = propertyAttribute.label;
                        EditorGUI.PropertyField(position, property, label);
                    }
                    catch (System.Exception ex) { Debug.LogException(ex); }
                }
            }

          
#endif
        }

        public class EnumFlagsAttribute : PropertyAttribute
        {
            public EnumFlagsAttribute() { }
        }

#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
        public class EnumFlagsAttributeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
            {
                _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
            }
        }

#endif

        [Header("Basic Settings")]

        /// <summary>
        /// Exposed in the Unity Editor to configure this behaviour
        /// </summary>
		[Tooltip("Your Bugsnag API Key from the project settings page.")]
        public string APIKey = "";

        /// <summary>
        /// Exposed in the Unity Editor to configure this behaviour
        /// </summary>
        [Tooltip("Should Bugsnag automatically send events to the dashboard.")]
        public bool AutoNotify = true;

        /// <summary>
        /// Exposed in the Unity Editor to configure this behaviour
        /// </summary>
		[Tooltip("Should Bugsnag automatically detect Android not responding errors.")]
        [LabelOverride("Auto Detect ANRs")]
        public bool AutoDetectAnrs = true;

        [Tooltip("Should Bugsnag automatically collect data about sessions.")]
        public bool AutoCaptureSessions = true;

        public LogType NotifyLevel = LogType.Exception;

        [Header("Advanced Settings")]

        [Tooltip("The number seconds required between unique Unity logs that Bugsnag will convert to breadcrumbs or report as errors (if configured). Increase the value to reduce the number of logs sent.")]
        public int SecondsPerUniqueLog = 5;

        [Range(0, 100)]
        public int MaximumBreadcrumbs = 25;

        [EnumFlags]
        public BreadcrumbType EnabledBreadcrumbTypes = (BreadcrumbType)(-1);

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// We use this to pull the fields that have been set in the
        /// Unity editor and pass them to the Bugsnag client.
        /// </summary>
        void Awake()
        {
            Configuration config = new Configuration(APIKey);
            config.AutoNotify = AutoNotify;
            config.AutoDetectAnrs = AutoNotify && AutoDetectAnrs;
            config.AutoCaptureSessions = AutoCaptureSessions;
            config.UniqueLogsTimePeriod = TimeSpan.FromSeconds(SecondsPerUniqueLog);
            config.NotifyLevel = NotifyLevel;
            config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
            config.MaximumBreadcrumbs = MaximumBreadcrumbs;
            config.ScriptingBackend = FindScriptingBackend();
            config.DotnetScriptingRuntime = FindDotnetScriptingRuntime();
            config.DotnetApiCompatibility = FindDotnetApiCompatibility();
            config.EnabledBreadcrumbTypes = GetEnabledBreadcrumbTypes();
            Bugsnag.Start(config);
        }


        private BreadcrumbType[] GetEnabledBreadcrumbTypes()
        {
            List<BreadcrumbType> selectedElements = new List<BreadcrumbType>();

            for (int i = 0; i < Enum.GetValues(typeof(BreadcrumbType)).Length; i++)
            {
                int layer = 1 << i;
                if (((int)EnabledBreadcrumbTypes & layer) != 0)
                {
                    var selectedType = (BreadcrumbType)Enum.GetValues(typeof(BreadcrumbType)).GetValue(i);
                    selectedElements.Add(selectedType);
                }
            }

            var allSelected = selectedElements.Count == Enum.GetValues(typeof(BreadcrumbType)).Length;
            //Notifier spec suggests that the default should be null which enables all types
            return allSelected ? null : selectedElements.ToArray();
        }

        /*** Determine runtime versions ***/

        private static string FindScriptingBackend()
        {
#if ENABLE_MONO
            return "Mono";
#elif ENABLE_IL2CPP
      return "IL2CPP";
#else
            return "Unknown";
#endif
        }

        private static string FindDotnetScriptingRuntime()
        {
#if NET_4_6
      return ".NET 4.6 equivalent";
#else
            return ".NET 3.5 equivalent";
#endif
        }

        private static string FindDotnetApiCompatibility()
        {
#if NET_2_0_SUBSET
      return ".NET 2.0 Subset";
#else
            return ".NET 2.0";
#endif
        }


#if UNITY_EDITOR

#if UNITY_IOS || UNITY_TVOS
        [PostProcessBuild(1400)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            var xcodeProjectPath = Path.Combine(path, "Unity-iPhone.xcodeproj");
            var pbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");
            var lines = new LinkedList<string>(File.ReadAllLines(pbxPath));
            BugsnagUnity.PostProcessBuild.Apply(lines);
            File.WriteAllLines(pbxPath, lines.ToArray());
        }
#endif

#endif
    }
}
