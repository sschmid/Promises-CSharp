using UnityEngine;
using Promises;
using System.Threading;

public class PromiseCleansUpOnFulfilledTest : MonoBehaviour {

    public int promises;

    PromiseService _promiseService;

    void Start() {
        Promise.WithAction(() => {
            Thread.Sleep(100);
            return 42;
        }).QueueOnMainThread(null, onFailed);

        _promiseService = UnityEngine.Object.FindObjectOfType<PromiseService>();

    }

    void Update() {
        promises = _promiseService.promises;
    }

    void onFailed(System.Exception error) {
    }
}

