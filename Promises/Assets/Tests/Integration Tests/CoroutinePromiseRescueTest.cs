using System.Collections;
using UnityEngine;
using Promises;
using System;

public class CoroutinePromiseRescueTest : MonoBehaviour {

    public int result;

    void Start() {
        Promise.WithCoroutine<int>(coroutine)
            .RescueCoroutine(rescueCoroutine)
            .OnFulfilled += r => result = r;
    }

    IEnumerator coroutine() {
        yield return new WaitForSeconds(0.1f);
        string s = null;
        s.ToUpper();
        yield return s;
    }

    IEnumerator rescueCoroutine(Exception ex) {
        var startIndex = 50;
        var endIndex = startIndex + 10;
        while (true) {
            if (startIndex == endIndex) {
                yield break;
            }
            startIndex++;
            yield return startIndex;
        }
    }
}

