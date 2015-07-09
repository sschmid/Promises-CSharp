using System.Collections;
using UnityEngine;
using Promises;

public class CoroutinePromiseOnFulfilledCalledTest : MonoBehaviour {

    public int result;

    void Start() {
        MainThreadDispatcher.Init();
        Promise.WithCoroutine<int>(coroutine).OnFulfilled += r => result = r;
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

