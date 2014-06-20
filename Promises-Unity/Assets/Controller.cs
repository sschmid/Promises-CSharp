using UnityEngine;
using System.Threading;
using Promises;

public class Controller : MonoBehaviour {
    void Start() {
        Invoke("makePromise", 1f);
    }

    void makePromise() {
        var promise = Promise<string>.PromiseWithAction(() => {
            Thread.Sleep(500);
            return "1";
        }).Then(result => {
            Thread.Sleep(500);
            return result + ", 2";
        }).Then(result => {
            Thread.Sleep(500);
            return result + ", 3";
        }).Then(result => {
            Thread.Sleep(500);
            return "Hello from a promise " + result + " and 4";
        });

        var wrapper = PromiseWrapper.Wrap(promise);
        wrapper.OnFulfilled += result => new GameObject((string)result);
    }
}
