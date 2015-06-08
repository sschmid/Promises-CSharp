using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Promises {

    public class MainThreadDispatcher : MonoBehaviour {

        public static bool isOnMainThread { get { return _mainThreadId == Thread.CurrentThread.ManagedThreadId; } }

        static bool _isInitialized;
        static int _mainThreadId;
        static List<Action> _actions;
        static MainThreadDispatcher _dispatcher;

        public static void Init() {
            if (!_isInitialized) {
                _dispatcher = new GameObject(typeof(MainThreadDispatcher).Name).AddComponent<MainThreadDispatcher>();
                UnityEngine.Object.DontDestroyOnLoad(_dispatcher);
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
                _actions = new List<Action>();
                _isInitialized = true;
            }
        }

        public static void Dispatch(Action action) {
            if (!_isInitialized) {
                throw new Exception("MainThreadDispatcher has to be initialized from the main thread first!");
            }

            if (isOnMainThread) {
                action();
            } else {
                lock (_actions) {
                    _actions.Add(action);
                }
            }
        }

        void Update() {
            Action[] actions = null;
            lock (_actions) {
                if (_actions.Count > 0) {
                    actions = _actions.ToArray();
                    _actions.Clear();
                }
            }
            if (actions != null) {
                foreach (var action in actions) {
                    action();
                }
            }
        }
    }
}