using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BugsnagUnity
{
    public class BugsnagCoroutineRunner : MonoBehaviour
    {
        private static BugsnagCoroutineRunner _instance;

        public static BugsnagCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var runnerObject = new GameObject("BugsnagCoroutineRunner");
                    _instance = runnerObject.AddComponent<BugsnagCoroutineRunner>();
                    DontDestroyOnLoad(runnerObject);
                }
                return _instance;
            }
        }
    }
    public class MainThreadDispatchBehaviour
    {

        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var newSystem = new PlayerLoopSystem
            {
                updateDelegate = OnUpdate
            };

            var systems = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            systems.Insert(0, newSystem);
            playerLoop.subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void OnUpdate()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public static void Enqueue(IEnumerator action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() =>
                {
                    BugsnagCoroutineRunner.Instance.StartCoroutine(action);
                });
            }
        }

        public static void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}
