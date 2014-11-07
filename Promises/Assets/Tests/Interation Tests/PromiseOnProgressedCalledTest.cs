using UnityEngine;
using Promises;
using System.Threading;

public class PromiseOnProgressedCalledTest : MonoBehaviour {

    public float progress;

    void Start() {
        var d = new Deferred<int>();
        d.action = () => {
            while (d.progress < 1) {
                Thread.Sleep(100);
                d.Progress(d.progress + 0.4f);
            }

            return 42;
        };

        d.RunAsync().QueueOnMainThread(null, null, onProgressed);
    }

    void onProgressed(float p) {
        progress = p;
    }
}

