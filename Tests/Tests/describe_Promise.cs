using NSpec;
using System;
using Promises;
using System.Threading;

class describe_Promise : nspec {

    const int shortDuration = 2;
    const int actionDuration = 4;
    const int actionDurationPlus = 6;

    void when_created() {
        Promise<string> promise = null;
        string eventResult = null;
        Exception eventError = null;
        var eventProgress = 0f;
        var fulfilledCalled = false;
        var failedCalled = false;

        context["cheap action"] = () => {
            before = () => {
                eventResult = null;
                eventError = null;
                eventProgress = 0f;
                fulfilledCalled = false;
                failedCalled = false;
            };

            after = () => promise.Join();

            context["when fulfilled"] = () => {
                before = () => {
                    promise = TestHelper.PromiseWithResult("42");
                    promise.OnFulfilled += result => eventResult = result;
                    promise.OnFailed += error => failedCalled = true;
                    promise.OnProgressed += progress => eventProgress = progress;
                    Thread.Sleep(shortDuration);
                };
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

            context["when failed"] = () => {
                before = () => {
                    promise = TestHelper.PromiseWithError<string>("error 42");
                    promise.OnFulfilled += result => fulfilledCalled = true;
                    promise.OnFailed += error => eventError = error;
                    Thread.Sleep(shortDuration);
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
                };
            };
        };

        context["progress"] = () => {
            Deferred<string> deferred = null;
            var progressEventCalled = 0;

            before = () => {
                eventProgress = 0f;
                progressEventCalled = 0;
                deferred = new Deferred<string>();
                deferred.OnProgressed += progress => {
                    eventProgress = progress;
                    progressEventCalled++;
                };
            };

            after = () => promise.Join();

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

            it["doesn't call OnProgressed when adding callback when progress is less than 1"] = () => {
                deferred.Progress(0.3f);
                var called = false;
                deferred.OnProgressed += progress => called = true;
                called.should_be_false();
            };

            it["calls OnProgressed when adding callback when progress is 1"] = () => {
                deferred.Progress(1f);
                var called = false;
                deferred.OnProgressed += progress => called = true;
                called.should_be_true();
            };
        };

        context["expensive action"] = () => {
            before = () => {
                eventResult = null;
                eventError = null;
                fulfilledCalled = false;
                failedCalled = false;
                promise = TestHelper.PromiseWithResult("42", actionDuration);
                promise.OnFulfilled += result => fulfilledCalled = true;
                promise.OnFailed += error => failedCalled = true;
            };

            after = () => promise.Join();

            context["initial state"] = () => {
                it["is unfulfilled"] = () => promise.state.should_be(PromiseState.Unfulfilled);
                it["has progressed 0%"] = () => promise.progress.should_be(0f);
                it["has no result"] = () => promise.result.should_be_null();
                it["has no error"] = () => promise.error.should_be_null();
                it["doesn't call OnFulfilled"] = () => fulfilledCalled.should_be_false();
                it["has a thread assigned"] = () => {
                    promise.thread.should_not_be_null();
                    promise.thread.should_not_be(Thread.CurrentThread);
                };
            };

            context["future state"] = () => {
                before = () => Thread.Sleep(actionDurationPlus);
                it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
                it["has progressed 100%"] = () => promise.progress.should_be(1f);
                it["has result"] = () => promise.result.should_be("42");
                it["has no error"] = () => promise.error.should_be_null();
                it["called OnFulfilled"] = () => fulfilledCalled.should_be_true();
                it["has no thread assigned"] = () => promise.thread.should_be_null();
            };

            context["join"] = () => {
                it["joins threads"] = () => {
                    promise.Join();
                    promise.state.should_be(PromiseState.Fulfilled);
                };

                it["does nothing when promise complete"] = () => {
                    promise.Join();
                    promise.Join();
                };
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
                promise.ToString().should_be("[Promise<String>: state = Fulfilled, result = 42]");
            };

            it["returns description of failed promise"] = () => {
                promise = TestHelper.PromiseWithError<string>("error 42");
                promise.Join();
                promise.ToString().should_be("[Promise<String>: state = Failed, progress = 0, error = error 42]");
            };
        };
    }
}