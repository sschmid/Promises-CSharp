using NSpec;
using Promises;
using System.Collections.Generic;

class describe_All : nspec {

    const int delay = 5;

    void when_running_in_parallel_with_all() {

        Promise<object> p1 = null;
        Promise<object> p2 = null;
        Promise<object[]> promise = null;
        List<float> eventProgresses = null;
        
        context["when all promises fulfill"] = () => {
            before = () => {
                eventProgresses = new List<float>();
                p1 = TestHelper.PromiseWithResult<object>(42, delay);
                p2 = TestHelper.PromiseWithResult<object>("42", 2 * delay);
                promise = Promise.All(p1, p2);
                promise.OnProgressed += eventProgresses.Add;
                promise.Await();
            };

            it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
            it["has progressed 100%"] = () => promise.progress.should_be(1f);
            it["has result"] = () => promise.result.should_not_be_null();
            it["has no error"] = () => promise.error.should_be_null();
            it["has no thread assigned"] = () => promise.thread.should_be_null();
            it["has results at correct index"] = () => {
                (promise.result[0]).should_be(42);
                (promise.result[1]).should_be("42");
            };

            it["calls progress"] = () => {
                eventProgresses.Count.should_be(2);
                eventProgresses[0].should_be(0.5f);
                eventProgresses[1].should_be(1f);
            };
        };

        context["when a promise fails"] = () => {

            before = () => {
                eventProgresses = new List<float>();
                p1 = TestHelper.PromiseWithResult<object>(42, delay);
                p2 = TestHelper.PromiseWithError<object>("error 42", 2 * delay);
                promise = Promise.All(p1, p2);
                promise.OnProgressed += eventProgresses.Add;
                promise.Await();
            };

            it["failed"] = () => promise.state.should_be(PromiseState.Failed);
            it["has progressed 50%"] = () => promise.progress.should_be(0.5f);
            it["has no result"] = () => promise.result.should_be_null();
            it["has error"] = () => promise.error.Message.should_be("error 42");
            it["has no thread assigned"] = () => promise.thread.should_be_null();
            it["calls progress"] = () => {
                eventProgresses.Count.should_be(1);
                eventProgresses[0].should_be(0.5f);
            };
        };
    }
}

