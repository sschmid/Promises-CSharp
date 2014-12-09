using System;
using System.Collections;
using UnityEngine;
using Promises;

public class CoroutineGetsExceptionTest : MonoBehaviour {

    public Exception exception;

    void Start() {
        CoroutineRunner.StartRoutine<int>(coroutine(), onComplete);
    }

    void onComplete(Coroutine<int> c) {
        try {
            var result = c.returnValue;
        } catch (System.Exception ex) {
            exception = ex;
        }
    }

    IEnumerator coroutine() {
        yield return new WaitForSeconds(0.1f);
        string s = null;
        s.ToUpper();
        yield return s;
    }
}

