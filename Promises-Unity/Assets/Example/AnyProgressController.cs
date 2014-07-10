using UnityEngine;
using Promises;
using System.Threading;
using System;
using System.Collections.Generic;

public class AnyProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = GetAnyPromise().QueueOnMainThread();
        promise.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        promise.OnFulfilled += result => new GameObject("Any done");
    }

    public static Promise<int> GetAnyPromise() {
        var promises = new List<Promise<int>>();
        for (int i = 0; i < 10; i++) {
            var localIndex = i + 1;
            promises.Add(Promise.WithAction(() => {
                Thread.Sleep(localIndex * 500);
                return 0;
            }));
        }
        return Promise.Any(promises.ToArray());
    }

    int sleepAction(int result) {
        Thread.Sleep(500);
        throw new Exception();
    }
}
