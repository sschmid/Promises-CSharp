using UnityEngine;
using Promises;
using System.Threading;
using System;

public class AllTogetherProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        getAllTogether().QueueOnMainThread(
            result => new GameObject("All together done"),
            null,
            progress => transform.localScale = new Vector3(progress * 10, 1f, 1f)
        );
    }

    Promise<object[]> getAllTogether() {
        var all = AllProgressController.GetAllPromise().Wrap<object>();
        var any = AnyProgressController.GetAnyPromise().Wrap<object>();
        var collect = CollectProgressController.GetCollectPromise().Wrap<object>();
        var deferred = DeferredProgressController.GetDeferred().Wrap<object>();
        var rescue = RescueProgressController.GetRescuePromise().Wrap<object>();
        var then = ThenProgressController.GetTenWithThen().Wrap<object>();
        var thenCoroutine = ThenCoroutineController.GetTenWithThenCoroutine().Wrap<object>();
        return Promise.Collect(all, any, collect, deferred, rescue, then, thenCoroutine);
    }

    int sleepAction(int result) {
        Thread.Sleep(500);
        throw new Exception();
    }
}
