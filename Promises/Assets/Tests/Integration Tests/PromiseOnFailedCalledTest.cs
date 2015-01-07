using UnityEngine;
using Promises;
using System.Threading;
using System;

public class PromiseOnFailedCalledTest : MonoBehaviour {

    public string errorMsg;

    void Start() {
        Promise.WithAction(() => {
            Thread.Sleep(100);
            throw new Exception("error 42");
            return 42;
        }).QueueOnMainThread(null, onFailed);
    }

    void onFailed(Exception e) {
        errorMsg = e.Message;
    }
}

