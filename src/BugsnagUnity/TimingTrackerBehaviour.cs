using UnityEngine;
using System.Collections;

namespace BugsnagUnity
{
    /// <summary>
    /// Manages events related to application state such as whether the app
    /// is in the foreground or background to improve report metadata.
    class TimingTrackerBehaviour : MonoBehaviour
    {

        private void Awake()
        {
            // Make sure that the tracker persists accross scenes.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// OnApplicationFocus is called when the application loses or gains focus.
        /// Alt-tabbing or Cmd-tabbing can take focus away from the Unity
        /// application to another desktop application. This causes the GameObjects
        /// to receive an OnApplicationFocus call with the argument set to false.
        /// When the user switches back to the Unity application, the GameObjects
        /// receive an OnApplicationFocus call with the argument set to true.
        /// </summary>
        /// <param name="hasFocus"></param>
        void OnApplicationFocus(bool hasFocus)
        {
            Bugsnag.SetApplicationState(hasFocus);
        }

        void OnApplicationPause(bool paused)
        {
            var hasFocus = !paused;
            Bugsnag.SetApplicationState(hasFocus);
        }
    }
}
