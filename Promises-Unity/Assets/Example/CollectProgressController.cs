using UnityEngine;
using Promises;
using System.Threading;
using System;
using System.Collections.Generic;

public class CollectProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = GetCollectPromise();
        var wrapper = PromiseWrapper.Wrap(promise, "Collect");
        wrapper.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        wrapper.OnFulfilled += result => new GameObject("Collect done");
    }

    public static Promise<object[]> GetCollectPromise() {
        var promises = new List<Promise<object>>();
        for (int i = 0; i < 5; i++) {
            var localIndex = i + 1;
            promises.Add(Promise.WithAction(() => {
                Thread.Sleep(localIndex * 500);
                return new object();
            }));
        }
        for (int i = 5; i < 10; i++) {
            var localIndex = i + 1;
            promises.Add(Promise.WithAction<object>(() => {
                Thread.Sleep(localIndex * 500);
                throw new Exception();
            }));
        }
        return Promise.Collect(promises.ToArray());
    }

    int sleepAction(int result) {
        Thread.Sleep(500);
        throw new Exception();
    }
}
