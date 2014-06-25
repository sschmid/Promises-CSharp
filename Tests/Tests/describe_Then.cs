using NSpec;
using Promises;
using System;
using System.Threading;

class describe_Then : nspec {

    const int delay = 5;

    void when_concatenating_with_then() {
        Promise<int> firstPromise = null;
        Promise<string> thenPromise = null;

        after = () => {
            firstPromise.Await();
            thenPromise.Await();
        };

        context["when first promise fulfilles"] = () => {

            before = () => {
                firstPromise = TestHelper.PromiseWithResult(42, delay);
                thenPromise = firstPromise.Then(result => {
                    Thread.Sleep(delay);
                    return result + "_then";
                });
            };

            context["before first promise finished"] = () => {
                it["first promise is running"] = () => assertRunning(firstPromise, 0f);
                it["then promise is pending"] = () => assertPending(thenPromise);
                it["then promise waits"] = () => {
                    thenPromise.Await();
                    assertFulfilled(firstPromise, 42);
                    assertFulfilled(thenPromise, "42_then");
                };

                context["when first promise finished"] = () => {
                    before = () => firstPromise.Await();

                    it["first promise is fulfilled"] = () => assertFulfilled(firstPromise, 42);
                    it["then promise in running with correct progress"] = () => assertRunning(thenPromise, 0.5f);

                    context["when then promise finished"] = () => {
                        before = () => thenPromise.Await();
                        it["then promise is fulfilled"] = () => assertFulfilled(thenPromise, "42_then");
                    };
                };
            };
        };

        context["when first promise fails"] = () => {

            before = () => {
                firstPromise = TestHelper.PromiseWithError<int>("error 42", delay);
                thenPromise = firstPromise.Then(result => {
                    Thread.Sleep(delay);
                    return result + "_then";
                });
                firstPromise.Await();
            };

            it["first promise failed"] = () => assertFailed(firstPromise, "error 42");
            it["then promise failed"] = () => assertFailed(thenPromise, "error 42");
        };

        context["when putting it all together"] = () => {
            
            it["fulfills all promises and passes result"] = () => {
                var promise = Promise.WithAction(() => "1")
                    .Then(result => result + "2")
                    .Then(result => result + "3")
                    .Then(result => result + "4");

                var fulfilled = 0;
                promise.OnFulfilled += result => fulfilled++;
                promise.Await();
                fulfilled.should_be(1);
                promise.result.should_be("1234");
            };

            it["forwards error"] = () => {
                var promise = TestHelper.PromiseWithError<string>("error 42")
                    .Then(result => result + "2")
                    .Then(result => result + "3")
                    .Then(result => result + "4");

                var fulfilled = false;
                Exception eventError = null;
                promise.OnFulfilled += result => fulfilled = true;
                promise.OnFailed += error => eventError = error;
                promise.Await();
                fulfilled.should_be_false();
                eventError.Message.should_be("error 42");
                promise.result.should_be_null();
                promise.error.Message.should_be("error 42");
            };

            it["calculates correct progress"] = () => {
                var promise = Promise.WithAction(() => {
                    Thread.Sleep(delay);
                    return "1";
                }).Then(result => result + "2")
                    .Then(result => result + "3")
                    .Then<string>(result => {
                    throw new Exception("error 42");
                });

                int eventCalled = 0;
                var expectedProgresses = new [] { 0.25f, 0.5f, 0.75, 1f };
                promise.OnProgressed += progress => {
                    progress.should_be(expectedProgresses[eventCalled]);
                    eventCalled++;
                };
                promise.Await();
                eventCalled.should_be(expectedProgresses.Length - 1);
            };

            it["calculates correct progress with custum progress"] = () => {
                var deferred1 = new Deferred<int>();
                deferred1.action = () => {
                    Thread.Sleep(delay);
                    var progress = 0f;
                    while (progress < 1f) {
                        progress += 0.25f;
                        deferred1.Progress(progress);
                        Thread.Sleep(delay);
                    }
                    return 0;
                };

                var deferred2 = new Deferred<int>();
                deferred2.action = () => {
                    var progress = 0f;
                    while (progress < 1f) {
                        progress += 0.25f;
                        deferred2.Progress(progress);
                        Thread.Sleep(delay);
                    }
                    return 0;
                };

                int eventCalled = 0;
                var expectedProgresses = new [] {
                    0.125f, 0.25f, 0.375, 0.5f,
                    0.625f, 0.75f, 0.875f, 1f,
                };

                var promise = deferred1.RunAsync().Then(deferred2);
                promise.OnProgressed += progress => {
                    progress.should_be(expectedProgresses[eventCalled]);
                    eventCalled++;
                };
                promise.Await();
                eventCalled.should_be(expectedProgresses.Length);
            };
        };
    }

    void assertRunning<TResult>(Promise<TResult> p, float progress) {
        p.state.should_be(PromiseState.Unfulfilled);
        p.error.should_be_null();
        p.progress.should_be(progress);
        p.thread.should_not_be_null();

        if (p.result is int)
            p.result.should_be(0);
        else
            ((object)p.result).should_be_null();
    }

    void assertPending<TResult>(Promise<TResult> p) {
        p.state.should_be(PromiseState.Unfulfilled);
        p.error.should_be_null();
        p.progress.should_be(0f);
        p.thread.should_be_null();

        if (p.result is int)
            p.result.should_be(0);
        else
            ((object)p.result).should_be_null();
    }

    void assertFulfilled<TResult>(Promise<TResult> p, TResult result) {
        p.state.should_be(PromiseState.Fulfilled);
        p.error.should_be_null();
        p.progress.should_be(1f);
        p.thread.should_be_null();
        p.result.should_be(result);
    }

    void assertFailed<TResult>(Promise<TResult> p, string errorMessage) {
        p.state.should_be(PromiseState.Failed);
        p.error.should_not_be_null();
        p.error.Message.should_be(errorMessage);
        p.thread.should_be_null();

        if (p.result is int)
            p.result.should_be(0);
        else
            ((object)p.result).should_be_null();
    }
}

