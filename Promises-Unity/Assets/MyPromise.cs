using UnityEngine;
using System.Threading;

public class MyPromise : PromiseBehaviour {
    void Awake() {
        Init(() => {
            Thread.Sleep(2000);
            return "Hello from MyPromise";
        }, promise => {
            Debug.Log(promise.result);
            new GameObject((string)promise.result);
        });
    }
}
