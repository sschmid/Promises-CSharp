using NSpec;
using System;
using Promises;
using System.Threading;

class describe_Promise : nspec {

    const int shortDuration = 5;
    const int actionDuration = 10;
    const int actionDurationPlus = 15;

    void when_created() {
        Promise<string> promise = null;
        string result = null;
        Exception error = null;
        var fulfilledCalled = false;
        var failedCalled = false;

        context["cheap action"] = () => {
            before = () => {
                result = null;
                error = null;
                fulfilledCalled = false;
                failedCalled = false;
            };

            context["when fulfilled"] = () => {
                before = () => {
                    promise = promiseWithResult<string>("42");
                    promise.OnFulfilled += r => result = r;
                    promise.OnFailed += e => failedCalled = true;
                    Thread.Sleep(shortDuration);
                };
                it["is fulfilled"] = () => promise.state.should_be(PromiseState.Fulfilled);
                it["has progressed 100%"] = () => promise.progress.should_be(1f);
                it["has result"] = () => promise.result.should_be("42");
                it["has no error"] = () => promise.error.should_be_null();
                it["has no thread assigned"] = () => promise.thread.should_be_null();

                context["events"] = () => {
                    it["calls OnFulfilled"] = () => result.should_be("42");
                    it["calls OnFulfilled when adding callback"] = () => {
                        string lateResult = null;
                        promise.OnFulfilled += r => lateResult = r;
                        lateResult.should_be("42");
                    };
                    it["doesn't call OnFailed"] = () => failedCalled.should_be_false();
                    it["doesn't call OnFailed when adding callback"] = () => {
                        var called = false;
                        promise.OnFailed += e => called = true;
                        called.should_be_false();
                    };
                };
            };

            context["when failed"] = () => {
                before = () => {
                    promise = promiseWithError<string>("error 42");
                    promise.OnFulfilled += r => fulfilledCalled = true;
                    promise.OnFailed += e => error = e;
                    Thread.Sleep(shortDuration);
                };
                it["failed"] = () => promise.state.should_be(PromiseState.Failed);
                it["has progressed 0%"] = () => promise.progress.should_be(0f);
                it["has no result"] = () => promise.result.should_be_null();
                it["has error"] = () => {
                    promise.error.should_not_be_null();
                    promise.error.Message.should_be("error 42");
                };
                it["has no thread assigned"] = () => promise.thread.should_be_null();

                context["events"] = () => {
                    it["doesn't call OnFulfilled"] = () => fulfilledCalled.should_be_false();
                    it["doesn't call OnFulfilled when adding callback"] = () => {
                        var called = false;
                        promise.OnFulfilled += r => called = true;
                        called.should_be_false();
                    };
                    it["calls OnFailed"] = () => {
                        error.should_not_be_null();
                        error.Message.should_be("error 42");
                    };
                    it["calls OnFailed when adding callback"] = () => {
                        Exception lateError = null;
                        promise.OnFailed += e => lateError = e;
                        lateError.should_not_be_null();
                        lateError.Message.should_be("error 42");
                    };
                };
            };
        };

        context["progress"] = () => {
            Deferred<string> deferred = null;
            var progress = 0f;
            var progressed = 0;

            before = () => {
                progress = 0f;
                progressed = 0;
                deferred = new Deferred<string>();
                deferred.OnProgressed += p => {
                    progress = p;
                    progressed++;
                };
            };

            it["progresses"] = () => {
                deferred.Progress(0.3f);
                deferred.Progress(0.6f);
                deferred.Progress(0.9f);
                deferred.Progress(1f);

                progress.should_be(1f);
                deferred.promise.progress.should_be(1f);
                progressed.should_be(4);
            };

            it["doesn't call OnProgressed when adding callback when progress is less than 1"] = () => {
                deferred.Progress(0.3f);
                var called = false;
                deferred.OnProgressed += p => called = true;
                called.should_be_false();
            };

            it["calls OnProgressed when adding callback when progress is 1"] = () => {
                deferred.Progress(1f);
                var called = false;
                deferred.OnProgressed += p => called = true;
                called.should_be_true();
            };
        };

        context["expensive action"] = () => {
            before = () => {
                result = null;
                error = null;
                fulfilledCalled = false;
                failedCalled = false;
                promise = promiseWithResult<string>("42", actionDuration);
                promise.OnFulfilled += r => fulfilledCalled = true;
                promise.OnFailed += e => failedCalled = true;
            };

            context["initial state"] = () => {
                after = () => promise.thread.Join();
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
        };
    }

    Promise<T> promiseWithResult<T>(T result) {
        return Promise<T>.PromiseWithAction(() => result);
    }

    Promise<T> promiseWithError<T>(string errorMessage) {
        return Promise<T>.PromiseWithAction(() => {
            throw new Exception(errorMessage);
        });
    }

    Promise<T> promiseWithResult<T>(T result, int delay) {
        return Promise<T>.PromiseWithAction(() => {
            Thread.Sleep(delay);
            return result;
        });
    }
}