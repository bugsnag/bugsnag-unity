using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.SceneManagement
{
    public struct Scene { }

    public class SceneManager
    {
        public static void add_sceneLoaded(UnityAction<Scene, LoadSceneMode> action) { }
    }
}