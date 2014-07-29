using UnityEngine;
using Promises;
using System.Threading;
using System;

public class AllTogetherProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = getAllTogether().QueueOnMainThread();
        promise.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        promise.OnFulfilled += result => new GameObject("All together done");
    }

    Promise<object[]> getAllTogether() {
        var all = AllProgressController.GetAllPromise().Wrap<object>();
        var any = AnyProgressController.GetAnyPromise().Wrap<object>();
        var collect = CollectProgressController.GetCollectPromise().Wrap<object>();
        var deferred = DeferredProgressController.GetDeferred().Wrap<object>();
        var rescue = RescueProgressController.GetRescuePromise().Wrap<object>();
        var then = ThenProgressController.GetTenWithThen().Wrap<object>();
        return Promise.Collect(all, any, collect, deferred, rescue, then);
    }

    int sleepAction(int result) {
        Thread.Sleep(500);
        throw new Exception();
    }
}
