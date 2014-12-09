using System.Collections;
using UnityEngine;
using Promises;

public class CoroutineGetsLastYieldTest : MonoBehaviour {

    public int result;

    void Start() {
        CoroutineRunner.StartRoutine<int>(coroutine(), onComplete);
    }

    void onComplete(Coroutine<int> c) {
        result = c.returnValue;
    }

    IEnumerator coroutine() {
        yield return new WaitForSeconds(0.1f);
        yield return null;
        yield return "hello";
        yield return 42;
    }
}

