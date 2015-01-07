using UnityEngine;
using System;
using System.Collections;

namespace Promises {
    public class CoroutineRunner : MonoBehaviour {
        static CoroutineRunner _coroutineRunner;

        public static Coroutine<T> StartRoutine<T>(IEnumerator coroutine, Action<Coroutine<T>> onComplete = null) {
            if (_coroutineRunner == null) {
                _coroutineRunner = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
            }

            return _coroutineRunner.StartCoroutine<T>(coroutine, onComplete);
        }

        public Coroutine<T> StartCoroutine<T>(IEnumerator coroutine, Action<Coroutine<T>> onComplete = null) {
            Coroutine<T> coroutineObject = new Coroutine<T>();
            coroutineObject.coroutine = StartCoroutine(coroutineObject.WrapRoutine(coroutine));
            if (onComplete != null) {
                StartCoroutine(onCompleteCoroutine<T>(coroutineObject, onComplete));
            }

            return coroutineObject;
        }

        IEnumerator onCompleteCoroutine<T>(Coroutine<T> coroutine, Action<Coroutine<T>> onComplete) {
            yield return coroutine.coroutine;
            onComplete(coroutine);
        }
    }
}