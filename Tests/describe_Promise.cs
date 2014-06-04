using NSpec;
using Promises;
using System.Threading;

class describe_Promise : nspec {
    void when_created() {
        Promise<int> promise = null;
        PromiseState state = PromiseState.Unfulfilled;

        context["cheap sync action"] = () => {
            before = () => {
                state = PromiseState.Unfulfilled;
                promise = Promise<int>.PromiseWithAction(cheapSyncAction);
                promise.OnStateChanged += p => state = p.state;
            };
            it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
            it["has 100% progressed"] = () => promise.progress.should_be(1f);
            it["has result"] = () => promise.result.should_not_be(0);
            it["executes action on a different thread"] = () => promise.result.should_not_be(Thread.CurrentThread.ManagedThreadId);
            it["calls onStateChange when fulfilled"] = () => state.should_be(PromiseState.Fulfilled);
            it["calls onStateChange on adding callback when already fulfilled"] = () => {
                var lateState = PromiseState.Unfulfilled;
                promise.OnStateChanged += p => lateState = p.state;
                promise.OnStateChanged += null;
                lateState.should_be(PromiseState.Fulfilled);
            };
            it["handles null callbacks"] = () => promise.OnStateChanged += null;
        };

        context["expensive sync action"] = () => {
            before = () => {
                state = PromiseState.Unfulfilled;
                promise = Promise<int>.PromiseWithAction(expensiveSyncAction);
                promise.OnStateChanged += p => state = p.state;
            };
            context["initial state"] = () => {
                it["is unfulfilled"] = () => promise.state.should_be(PromiseState.Unfulfilled);
                it["has 0% progressed"] = () => promise.progress.should_be(0f);
                it["has no result"] = () => promise.result.should_be(0);
                it["didn't call onStateChange yet"] = () => state.should_be(PromiseState.Unfulfilled);
            };
            context["future state"] = () => {
                before = () => Thread.Sleep(10);
                it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
                it["has 100% progressed"] = () => promise.progress.should_be(1f);
                it["has result"] = () => promise.result.should_not_be(0);
                it["executes action on a different thread"] = () => promise.result.should_not_be(Thread.CurrentThread.ManagedThreadId);
                it["calls onStateChange when fulfilled"] = () => state.should_be(PromiseState.Fulfilled);
            };
        };
    }

    int cheapSyncAction() {
        return Thread.CurrentThread.ManagedThreadId;
    }

    int expensiveSyncAction() {
        var a = 0;
        for (int i = 0; i < 5000000; i++)
            a++;
        return Thread.CurrentThread.ManagedThreadId;
    }
}