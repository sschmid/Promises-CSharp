using UnityEngine;
using Promises;
using System.Threading;

public class PromiseOnFulfilledCalledTest : MonoBehaviour {

    public int result;

    void Start() {
        Promise.WithAction(() => {
            Thread.Sleep(100);
            return 42;
        }).QueueOnMainThread(onFulfilled);
    }

    void onFulfilled(int r) {
        result = r;
    }
}

