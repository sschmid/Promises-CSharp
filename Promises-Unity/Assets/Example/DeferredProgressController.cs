using UnityEngine;
using Promises;
using System;
using System.Threading;

public class DeferredProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = getFourPromises();
        var wrapper = PromiseWrapper.Wrap(promise, "Deferred");
        wrapper.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        wrapper.OnFulfilled += result => new GameObject("4 promises done");
    }

    Promise<int> getFourPromises() {
        return customProgressPromise(true).Then(customProgressPromise()).Then(customProgressPromise()).Then(customProgressPromise());
    }

    Promise<int>customProgressPromise(bool autoStart = false) {
        var deferred = new Deferred<int>();
        deferred.action = () => {
            Thread.Sleep(1000);
            var progress = 0f;
            while (progress < 1f) {
                progress += 0.01f;
                progress = Math.Min(1f, progress);
                deferred.Progress(progress);
                Thread.Sleep(7);
            }
            return 0;
        };

        return autoStart ? deferred.RunAsync() : deferred.promise;
    }
}
