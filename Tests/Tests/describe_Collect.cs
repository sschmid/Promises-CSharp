using NSpec;
using Promises;
using System.Collections.Generic;

class describe_Collect : nspec {

    const int delay = 5;

    void when_running_in_parallel_with_collect() {
        Promise<object> p1 = null;
        Promise<object> p2 = null;
        Promise<object[]> promise = null;
        List<float> eventProgresses = null;

        context["when running with a promise that fulfills and one that fails"] = () => {
            before = () => {
                eventProgresses = new List<float>();
                p1 = TestHelper.PromiseWithError<object>("error 42", delay * 2);
                p2 = TestHelper.PromiseWithResult<object>("42", delay);
                promise = Promise.Collect(p1, p2);
                promise.OnProgressed += eventProgresses.Add;
                promise.Await();
            };

            it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
            it["has progressed 100%"] = () => promise.progress.should_be(1f);
            it["has result"] = () => promise.result.should_not_be_null();
            it["has no error"] = () => promise.error.should_be_null();
            it["has results at correct index"] = () => {
                (promise.result[0]).should_be_null();
                (promise.result[1]).should_be("42");
            };

            it["calls progress"] = () => {
                eventProgresses.Count.should_be(2);
                eventProgresses[0].should_be(0.5f);
                eventProgresses[1].should_be(1f);
            };

            it["has initial progress"] = () => {
                var deferred = new Deferred<object>();
                deferred.Progress(0.5f);
                p2 = TestHelper.PromiseWithResult<object>("42", delay);
                var collect = Promise.Collect(deferred, p2);
                collect.progress.should_be(0.25f);
                deferred.Fulfill(null);
                collect.Await();
            };
        };

        context["when all promises fail"] = () => {
            before = () => {
                eventProgresses = new List<float>();
                p1 = TestHelper.PromiseWithError<object>("error 42", delay * 2);
                p2 = TestHelper.PromiseWithError<object>("error 43", delay);
                promise = Promise.Collect(p1, p2);
                promise.OnProgressed += eventProgresses.Add;
                promise.Await();
            };

            it["progresses"] = () => {
                eventProgresses.Count.should_be(2);
                eventProgresses[0].should_be(0.5f);
                eventProgresses[1].should_be(1f);
            };
        };
    }
}

