using System.Collections;
using UnityEngine;
using Promises;

public class ThenCoroutineController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var p = GetTenWithThenCoroutine();
        p.OnFulfilled += result => new GameObject("Then Coroutine done");
        p.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
    }

    public static Promise<int> GetTenWithThenCoroutine() {
        var promise = Promise.WithCoroutine<int>(() => coroutine(0));

        for (int i = 0; i < 9; i++) {
            promise = promise.ThenCoroutine<int>(coroutine);
        }

        return promise;
    }

    static IEnumerator coroutine(int startIndex) {
        var endIndex = startIndex + 10;
        while (startIndex != endIndex) {
            startIndex++;
            yield return new WaitForSeconds(0.06f);
        }

        yield return startIndex;
    }
}
