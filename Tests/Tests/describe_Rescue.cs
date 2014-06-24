using NSpec;
using System;
using Promises;
using System.Threading;

class describe_Rescue : nspec {
    
    const int delay = 5;

    void when_using_rescue() {
        Promise<string> promise = null;
        string eventResult = null;

        before = () => eventResult = null;
        after = () => promise.Join();

        it["rescues failed promise"] = () => {
            promise = TestHelper.PromiseWithError<string>("error 42", delay).Rescue(error => "rescue");
            promise.OnFulfilled += result => eventResult = result;
            promise.Join();
            promise.state.should_be(PromiseState.Fulfilled);
            promise.progress.should_be(1f);
            promise.result.should_be("rescue");
            promise.error.should_be_null();
            eventResult.should_be("rescue");
        };

        it["fulfills when no error"] = () => {
            promise = TestHelper.PromiseWithResult("42", delay).Rescue(error => "rescue");
            promise.OnFulfilled += result => eventResult = result;
            promise.Join();
            Thread.SpinWait(0);
            promise.state.should_be(PromiseState.Fulfilled);
            promise.progress.should_be(1f);
            promise.result.should_be("42");
            promise.error.should_be_null();
            eventResult.should_be("42");
        };

        context["progress"] = () => {
            Deferred<string> deferred = null;
            var progressCalled = 0;
            var eventProgress = 0f;

            before = () => {
                eventProgress = 0f;
                progressCalled = 0;
                deferred = new Deferred<string>();
                promise = deferred.Rescue(error => "43");
                promise.OnProgressed += progress => {
                    eventProgress = progress;
                    progressCalled++;
                };
            };

            it["forwards progress"] = () => {
                deferred.Progress(0.3f);
                deferred.Progress(0.6f);

                eventProgress.should_be(0.6f);
                promise.progress.should_be(0.6f);
                progressCalled.should_be(2);

                deferred.Fulfill("42");
            };
        };
    }
}

