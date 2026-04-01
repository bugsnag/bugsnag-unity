using UnityEngine;

namespace BugsnagUnity
{
    /// <summary>
    /// Manages events related to application state such as whether the app
    /// is in the foreground or background to improve report metadata.
    /// </summary>
    class TimingTrackerBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            // Make sure that the tracker persists accross scenes.
            DontDestroyOnLoad(gameObject);

            // Initialize WebGL foreground detection if on WebGL platform
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                BugsnagWebGLBridge.Initialize();
            }
        }

        /// <summary>
        /// OnApplicationFocus is called when the application loses or gains focus.
        /// </summary>
        void OnApplicationFocus(bool hasFocus)
        {
            // On non-WebGL platforms, use the native callback
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Bugsnag.SetApplicationState(hasFocus);
            }
        }

        void OnApplicationPause(bool paused)
        {
            var hasFocus = !paused;

            // On non-WebGL platforms, use the native callback
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Bugsnag.SetApplicationState(hasFocus);
            }
        }

        // Called from WebGL JavaScript (BugsnagWebGL.jslib) via SendMessage
        // to update application foreground/background state based on
        // browser visibility and focus events.
        public void SetApplicationStateFromWebGL(string state)
        {
            // "1" = foreground, "0" = background
            bool hasFocus = state == "1";
            Bugsnag.SetApplicationState(hasFocus);
        }
    }
}
