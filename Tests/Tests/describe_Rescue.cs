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
        after = () => promise.Await();

        it["rescues failed promise"] = () => {
            promise = TestHelper.PromiseWithError<string>("error 42", delay).Rescue(error => "rescue");
            promise.OnFulfilled += result => eventResult = result;
            promise.Await();
            promise.state.should_be(PromiseState.Fulfilled);
            promise.progress.should_be(1f);
            promise.result.should_be("rescue");
            promise.error.should_be_null();
            eventResult.should_be("rescue");
        };

        it["fulfills when no error"] = () => {
            promise = TestHelper.PromiseWithResult("42", delay).Rescue(error => "rescue");
            promise.OnFulfilled += result => eventResult = result;
            promise.Await();
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
                deferred.Fulfill("42");

                eventProgress.should_be(1f);
                promise.progress.should_be(1f);
                progressCalled.should_be(3);
            };
        };

        it["calculates correct progress when concatenating with then"] = () => {
            Func<string, string> p = result => {
                Thread.Sleep(delay);
                return "42";
            };
            Func<Exception, string> r = error => {
                Thread.Sleep(delay);
                return "error";
            };

            promise = TestHelper.PromiseWithResult("42", delay)
                .Then(p)
                .Then(p)
                .Then(p).Rescue(r);


            int eventCalled = 0;
            var expectedProgresses = new [] { 0.25f, 0.5f, 0.75f, 1f };
            promise.OnProgressed += progress => {
                progress.should_be(expectedProgresses[eventCalled]);
                eventCalled++;
            };
            promise.Await();
            eventCalled.should_be(expectedProgresses.Length);
        };
    }
}

