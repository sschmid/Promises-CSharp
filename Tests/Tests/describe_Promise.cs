using NSpec;
using System;
using Promises;
using System.Threading;

class describe_Promise : nspec {

    const int delay = 5;

    void when_running_an_expensive_action() {
        Promise<string> promise = null;
        string eventResult = null;
        Exception eventError = null;
        float eventProgress = 0f;
        bool fulfilledCalled = false, failedCalled = false, progressCalled = false;

        before = () => {
            eventResult = null;
            eventError = null;
            eventProgress = -1f;
            fulfilledCalled = failedCalled = progressCalled = false;
        };

        after = () => promise.Await();

        context["before action finished"] = () => {

            before = () => {
                promise = TestHelper.PromiseWithResult("42", delay);
                promise.OnFulfilled += result => {
                    eventResult = result;
                    fulfilledCalled = true;
                };
                promise.OnFailed += error => {
                    eventError = error;
                    failedCalled = true;
                };
                promise.OnProgressed += progress => {
                    eventProgress = progress;
                    eventProgress = progress;
                };
            };

            it["is unfulfilled"] = () => promise.state.should_be(PromiseState.Unfulfilled);
            it["has progressed 0%"] = () => promise.progress.should_be(0f);
            it["has no result"] = () => promise.result.should_be_null();
            it["has no error"] = () => promise.error.should_be_null();
            it["has a background thread assigned"] = () => {
                promise.thread.should_not_be_null();
                promise.thread.should_not_be(Thread.CurrentThread);
                promise.thread.IsBackground.should_be_true();
            };

            context["await"] = () => {
                it["blocks until finished"] = () => {
                    promise.Await();
                    promise.thread.should_be_null();
                    promise.state.should_be(PromiseState.Fulfilled);
                };

                it["does nothing when promise already finished"] = () => {
                    promise.Await();
                    promise.Await();
                    true.should_be_true();
                };
            };

            context["events"] = () => {
                it["doesn't call OnFulfilled"] = () => fulfilledCalled.should_be_false();
                it["doesn't call OnFail"] = () => failedCalled.should_be_false();
                it["doesn't call OnProgress"] = () => progressCalled.should_be_false();
            };

            context["when action finished"] = () => {

                before = () => promise.Await();

                it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
                it["has progressed 100%"] = () => promise.progress.should_be(1f);
                it["has result"] = () => promise.result.should_be("42");
                it["has no error"] = () => promise.error.should_be_null();
                it["has no thread assigned"] = () => promise.thread.should_be_null();

                context["events"] = () => {
                    it["calls OnFulfilled"] = () => eventResult.should_be("42");
                    it["calls OnFulfilled when adding callback"] = () => {
                        string lateResult = null;
                        promise.OnFulfilled += result => lateResult = result;
                        lateResult.should_be("42");
                    };
                    it["doesn't call OnFailed"] = () => failedCalled.should_be_false();
                    it["doesn't call OnFailed when adding callback"] = () => {
                        var called = false;
                        promise.OnFailed += error => called = true;
                        called.should_be_false();
                    };
                    it["calls OnProgress"] = () => eventProgress.should_be(1f);
                };
            };
        };

        context["when action throws an exception"] = () => {

            before = () => {
                promise = TestHelper.PromiseWithError<string>("error 42", delay);
                promise.OnFulfilled += result => {
                    eventResult = result;
                    fulfilledCalled = true;
                };
                promise.OnFailed += error => {
                    eventError = error;
                    failedCalled = true;
                };
                promise.OnProgressed += progress => {
                    eventProgress = progress;
                    eventProgress = progress;
                };
                promise.Await();
            };

            it["failed"] = () => promise.state.should_be(PromiseState.Failed);
            it["has progressed 0%"] = () => promise.progress.should_be(0f);
            it["has no result"] = () => promise.result.should_be_null();
            it["has error"] = () => promise.error.Message.should_be("error 42");
            it["has no thread assigned"] = () => promise.thread.should_be_null();

            context["events"] = () => {
                it["doesn't call OnFulfilled"] = () => fulfilledCalled.should_be_false();
                it["doesn't call OnFulfilled when adding callback"] = () => {
                    var called = false;
                    promise.OnFulfilled += result => called = true;
                    called.should_be_false();
                };
                it["calls OnFailed"] = () => eventError.Message.should_be("error 42");
                it["calls OnFailed when adding callback"] = () => {
                    Exception lateError = null;
                    promise.OnFailed += error => lateError = error;
                    lateError.Message.should_be("error 42");
                };
                it["doesn't call OnProgress"] = () => progressCalled.should_be_false();
            };
        };

        context["when creating a custom promise with Deferred"] = () => {
            Deferred<string> deferred = null;
            int progressEventCalled = 0;

            before = () => {
                deferred = new Deferred<string>();
                progressEventCalled = 0;
                deferred.OnProgressed += progress => {
                    eventProgress = progress;
                    progressEventCalled++;
                };
            };

            it["progresses"] = () => {
                deferred.Progress(0.3f);
                deferred.Progress(0.6f);
                deferred.Progress(0.9f);
                deferred.Progress(1f);

                eventProgress.should_be(1f);
                deferred.promise.progress.should_be(1f);
                progressEventCalled.should_be(4);
            };

            it["doesn't call OnProgressed when setting equal progress"] = () => {
                deferred.Progress(0.3f);
                deferred.Progress(0.3f);
                deferred.Progress(0.3f);
                deferred.Progress(0.6f);

                eventProgress.should_be(0.6f);
                deferred.promise.progress.should_be(0.6f);
                progressEventCalled.should_be(2);
            };

            it["doesn't call OnProgressed when adding callback when progress is less than 100%"] = () => {
                deferred.Progress(0.3f);
                var called = false;
                deferred.OnProgressed += progress => called = true;
                called.should_be_false();
            };

            it["calls OnProgressed when adding callback when progress is 100%"] = () => {
                deferred.Progress(1f);
                var called = false;
                deferred.OnProgressed += progress => called = true;
                called.should_be_true();
            };
        };

        context["toString"] = () => {
            it["returns description of unfulfilled promise"] = () => {
                var deferred = new Deferred<string>();
                deferred.Progress(0.1234567890f);
                deferred.promise.ToString().should_be("[Promise<String>: state = Unfulfilled, progress = 0,123]");
            };

            it["returns description of fulfilled promise"] = () => {
                promise = TestHelper.PromiseWithResult("42");
                promise.Await();
                promise.ToString().should_be("[Promise<String>: state = Fulfilled, result = 42]");
            };

            it["returns description of failed promise"] = () => {
                promise = TestHelper.PromiseWithError<string>("error 42");
                promise.Await();
                promise.ToString().should_be("[Promise<String>: state = Failed, progress = 0, error = error 42]");
            };
        };
    }
}