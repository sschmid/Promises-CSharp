using UnityEngine;
using Promises;
using System.Threading;
using System;

public class PromiseCleansUpOnFailedTest : MonoBehaviour {

    public int promises;

    PromiseService _promiseService;

    void Start() {
        Promise.WithAction(() => {
            Thread.Sleep(100);
            throw new Exception("error 42");
            return 42;
        }).QueueOnMainThread(onFulfilled);

        _promiseService = UnityEngine.Object.FindObjectOfType<PromiseService>();
    }

    void Update() {
        promises = _promiseService.promises;
    }

    void onFulfilled(int result) {
    }
}

