using System.Collections;
using UnityEngine;
using Promises;

public class CoroutinePromiseThenTest : MonoBehaviour {

    public int result;

    void Start() {
        MainThreadDispatcher.Init();
        Promise.WithCoroutine<int>(coroutine)
            .ThenCoroutine<int>(thenCoroutine)
            .OnFulfilled += r => result = r;
    }

    IEnumerator coroutine() {
        var startIndex = 0;
        var endIndex = 10;
        while (true) {
            if (startIndex == endIndex) {
                yield break;
            }
            startIndex++;
            yield return startIndex;
        }
    }

    IEnumerator thenCoroutine(int startIndex) {
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

