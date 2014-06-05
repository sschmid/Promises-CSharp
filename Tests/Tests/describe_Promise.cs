using NSpec;
using Promises;
using System.Threading;

class describe_Promise : nspec {
    void when_created() {
        Promise<int> promise = null;
        PromiseState state = PromiseState.Unfulfilled;

        context["cheap action"] = () => {
            before = () => {
                state = PromiseState.Unfulfilled;
                promise = Promise<int>.PromiseWithAction(TestHelper.CheapAction);
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
                lateState.should_be(PromiseState.Fulfilled);
            };
        };

        context["expensive action"] = () => {
            before = () => {
                state = PromiseState.Unfulfilled;
                promise = Promise<int>.PromiseWithAction(TestHelper.ExpensiveAction);
                promise.OnStateChanged += p => state = p.state;
            };
            context["initial state"] = () => {
                it["is unfulfilled"] = () => promise.state.should_be(PromiseState.Unfulfilled);
                it["has 0% progressed"] = () => promise.progress.should_be(0f);
                it["has no result"] = () => promise.result.should_be(0);
                it["didn't call onStateChange yet"] = () => state.should_be(PromiseState.Unfulfilled);
                it["doesn't call onStateChange on adding callback when unfulfilled"] = () => {
                    var lateState = PromiseState.Fulfilled;
                    promise.OnStateChanged += p => lateState = p.state;
                    lateState.should_be(PromiseState.Fulfilled);
                };
            };
            context["future state"] = () => {
                before = () => Thread.Sleep(100);
                it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
                it["has 100% progressed"] = () => promise.progress.should_be(1f);
                it["has result"] = () => promise.result.should_not_be(0);
                it["executes action on a different thread"] = () => promise.result.should_not_be(Thread.CurrentThread.ManagedThreadId);
                it["calls onStateChange when fulfilled"] = () => state.should_be(PromiseState.Fulfilled);
            };
        };
    }
}