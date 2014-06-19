using NSpec;
using System;
using System.Threading;
using Promises;

class describe_Rescue : nspec {
    
    const int shortDuration = 5;
    const int actionDuration = 10;
    const int actionDurationPlus = 15;

    void when_rescue() {
        Promise<int> promise = null;
        int eventResult = 0;

        before = () => {
            eventResult = 0;            
        };

        it["rescues failed promise"] = () => {
            promise = TestHelper.PromiseWithError<int>("error 42").Rescue(error => 43);
            promise.OnFulfilled += result => eventResult = result;
            Thread.Sleep(shortDuration);
            promise.state.should_be(PromiseState.Fulfilled);
            promise.progress.should_be(1f);
            promise.result.should_be(43);
            promise.error.should_be_null();
            eventResult.should_be(43);
        };

        it["fulfills when no error"] = () => {
            promise = TestHelper.PromiseWithResult(42).Rescue(error => 43);
            promise.OnFulfilled += result => eventResult = result;
            Thread.Sleep(shortDuration);
            promise.state.should_be(PromiseState.Fulfilled);
            promise.progress.should_be(1f);
            promise.result.should_be(42);
            promise.error.should_be_null();
            eventResult.should_be(42);
        };

        context["progress"] = () => {
            Deferred<int> deferred = null;
            var progressEventCalled = 0;
            var eventProgress = 0f;

            before = () => {
                eventProgress = 0f;
                progressEventCalled = 0;
                deferred = new Deferred<int>();

                promise = deferred.Rescue(error => 43);
                promise.OnProgressed += progress => {
                    eventProgress = progress;
                    progressEventCalled++;
                };

            };

            it["forwards progress"] = () => {
                deferred.Progress(0.3f);
                deferred.Progress(0.6f);

                eventProgress.should_be(0.6f);
                promise.progress.should_be(0.6f);
                progressEventCalled.should_be(2);
            };
        };
    }
}

