using UnityEngine;
using Promises;
using System.Threading;

public class PromiseProgressController : MonoBehaviour {
    void Start() {
        transform.localScale = Vector3.zero;
        var promise = getTenPromises();
        var wrapper = PromiseWrapper.Wrap(promise);
        wrapper.OnProgressed += progress => transform.localScale = new Vector3(progress * 10, 1f, 1f);
        wrapper.OnFulfilled += result => new GameObject("10 promises done");
    }

    Promise<int> getTenPromises() {
        var promise = Promise.WithAction(() => {
            Thread.Sleep(500);
            return 0;
        });

        for (int i = 0; i < 9; i++)
            promise = promise.Then<int>(sleepAction);

        return promise;
    }

    int sleepAction(int result) {
        Thread.Sleep(500);
        return 0;
    }
}
