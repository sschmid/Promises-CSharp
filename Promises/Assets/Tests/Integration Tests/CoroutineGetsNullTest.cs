using System.Collections;
using UnityEngine;
using Promises;

public class CoroutineGetsNullTest : MonoBehaviour {

    public string result = "result";

    void Start() {
        CoroutineRunner.StartRoutine<string>(coroutine(), onComplete);
    }

    void onComplete(Coroutine<string> c) {
        result = c.returnValue;
    }

    IEnumerator coroutine() {
        yield return new WaitForSeconds(0.1f);
        yield return null;
    }
}

