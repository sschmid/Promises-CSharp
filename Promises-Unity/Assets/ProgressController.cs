using UnityEngine;
using Promises;
using System;
using System.Threading;

public class ProgressController : MonoBehaviour {
    void Start() {
        var promise = progressTest1();
//        var promise = progressTest2();

        var wrapper = PromiseWrapper.Wrap(promise);
        wrapper.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        wrapper.OnFulfilled += result => new GameObject("DONE");
    }

    Promise<int> progressTest1() {
        var promise = Promise<int>.PromiseWithAction(() => {
            Thread.Sleep(500);
            return 0; 
        })
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Rescue(error => 0)
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Rescue(error => 0)
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Rescue(error => 0)
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Rescue(error => 0)
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        })
            .Rescue(error => 0)
            .Then(result => {
            Thread.Sleep(500);
            return 0; 
        });

        return promise;
    }

    Promise<int> progressTest2() {
        return progressPromise(true).Then(progressPromise()).Then(progressPromise()).Then(progressPromise());
    }

    Promise<int>progressPromise(bool autoStart = false) {
        var deferred = new Deferred<int>();
        deferred.action = () => {
            Thread.Sleep(1000);
            var progress = 0f;
            while (progress < 1f) {
                progress += 0.01f;
                progress = Math.Min(1f, progress);
                deferred.Progress(progress);
                Thread.Sleep(30);
            }
            return 0;
        };

        return autoStart ? deferred.RunAsync() : deferred.promise;
    }
}
