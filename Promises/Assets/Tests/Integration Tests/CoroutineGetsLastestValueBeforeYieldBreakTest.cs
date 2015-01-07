using System.Collections;
using UnityEngine;
using Promises;

public class CoroutineGetsLastestValueBeforeYieldBreakTest : MonoBehaviour {

    public int result;

    void Start() {
        CoroutineRunner.StartRoutine<int>(coroutine(), onComplete);
    }

    void onComplete(Coroutine<int> c) {
        result = c.returnValue;
    }

    IEnumerator coroutine() {
        var i = 0;
        while (true) {
            if (i == 10) {
                yield break;
            }
            i++;
            yield return i;
        }
    }
}

