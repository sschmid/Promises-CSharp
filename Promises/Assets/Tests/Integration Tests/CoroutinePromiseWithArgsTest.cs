using System.Collections;
using UnityEngine;
using Promises;

public class CoroutinePromiseWithArgsTest : MonoBehaviour {

    public int result;

    void Start() {
        MainThreadDispatcher.Init();
        Promise.WithCoroutine<int>(() => coroutine(10)).OnFulfilled += r => result = r;
    }

    IEnumerator coroutine(int endIndex) {
        var i = 0;
        while (true) {
            if (i == endIndex) {
                yield break;
            }
            i++;
            yield return i;
        }
    }
}

