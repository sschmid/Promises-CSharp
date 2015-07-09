using System;
using System.Collections;
using UnityEngine;
using Promises;

public class CoroutinePromiseOnFailedCalledTest : MonoBehaviour {

    public Exception result;

    void Start() {
        MainThreadDispatcher.Init();
        Promise.WithCoroutine<int>(coroutine).OnFailed += error => result = error;
    }

    IEnumerator coroutine() {
        yield return new WaitForSeconds(0.1f);
        string s = null;
        s.ToUpper();
        yield return s;
    }
}

