using System.Collections;
using UnityEngine;
using Promises;

public class CoroutinePromiseOnProgressedCalledTest : MonoBehaviour {

    public float progress;

    void Start() {
        Promise.WithCoroutine<int>(coroutine).OnProgressed += p => progress = p;
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

