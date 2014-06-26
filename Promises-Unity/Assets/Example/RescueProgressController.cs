using UnityEngine;
using Promises;
using System.Threading;
using System;

public class RescueProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = GetRescuePromise();
        var wrapper = PromiseWrapper.Wrap(promise, "Rescue");
        wrapper.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        wrapper.OnFulfilled += result => new GameObject("Rescue done");
    }

    public static Promise<int> GetRescuePromise() {
        var promise = Promise.WithAction<int>(() => {
            Thread.Sleep(500);
            throw new Exception();
        }).Rescue(arg => 0);

        for (int i = 0; i < 9; i++)
            promise = promise.Then<int>(sleepAction).Rescue(arg => 0);

        return promise;
    }

    static int sleepAction(int result) {
        Thread.Sleep(500);
        throw new Exception();
    }
}
