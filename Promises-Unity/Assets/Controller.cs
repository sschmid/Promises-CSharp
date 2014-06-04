using UnityEngine;
using System.Threading;
using Promises;

public class Controller : MonoBehaviour {
    void Start() {
        PromiseBehaviour.PromiseWithAction(run, onStateChanged);
    }

    string run() {
        Thread.Sleep(2000);
        return "Hello from a promise";
    }

    void onStateChanged(Promise<object> promise) {
        Debug.Log(promise.result);
        new GameObject(promise.result as string);
    }
}
