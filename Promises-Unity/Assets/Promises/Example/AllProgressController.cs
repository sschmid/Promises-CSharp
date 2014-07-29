using UnityEngine;
using Promises;
using System.Threading;
using System;
using System.Collections.Generic;

public class AllProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = GetAllPromise().QueueOnMainThread();
        promise.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        promise.OnFulfilled += result => new GameObject("All done");
    }

    public static Promise<object[]> GetAllPromise() {
        var promises = new List<Promise<object>>();
        for (int i = 0; i < 10; i++) {
            var localIndex = i + 1;
            promises.Add(Promise.WithAction(() => {
                Thread.Sleep(localIndex * 500);
                return new object();
            }));
        }
        return Promise.All(promises.ToArray());
    }

    int sleepAction(int result) {
        Thread.Sleep(500);
        throw new Exception();
    }
}
