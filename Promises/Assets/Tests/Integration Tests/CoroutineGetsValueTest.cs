using System.Collections;
using UnityEngine;
using Promises;

public class CoroutineGetsValueTest : MonoBehaviour {

    public string result;

    void Start() {
        CoroutineRunner.StartRoutine<string>(coroutine(), onComplete);
    }

    void onComplete(Coroutine<string> c) {
        result = c.returnValue;
    }

    IEnumerator coroutine() {
        yield return new WaitForSeconds(0.1f);
        yield return "fourty-two";
    }
}

