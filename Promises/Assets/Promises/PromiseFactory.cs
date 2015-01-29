using System;
using System.Collections;
using System.Threading;

namespace Promises {
    public static class Promise {

        public static Promise<T> WithAction<T>(Func<T> action) {
            var deferred = new Deferred<T>();
            deferred.action = action;
            return deferred.RunAsync();
        }

        public static Promise<T> WithCoroutine<T>(Func<IEnumerator> coroutine) {
            var deferred = new Deferred<T>();
            deferred.coroutine = coroutine;
            return deferred.RunAsync();
        }

        public static Promise<object[]> All(params Promise<object>[] promises) {
            var deferred = new Deferred<object[]>();
            var results = new object[promises.Length];
            var done = 0;

            var initialProgress = 0f;
            foreach (var p in promises) {
                initialProgress += p.progress;
            }

            deferred.Progress(initialProgress / (float)promises.Length);

            for (int i = 0, promisesLength = promises.Length; i < promisesLength; i++) {
                var localIndex = i;
                var promise = promises[localIndex];
                promise.OnFulfilled += result => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        results[localIndex] = result;
                        Interlocked.Increment(ref done);
                        if (done == promisesLength) {
                            deferred.Fulfill(results);
                        }
                    }
                };
                promise.OnFailed += error => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        deferred.Fail(error);
                    }
                };
                promise.OnProgressed += progress => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        var totalProgress = 0f;
                        foreach (var p in promises) {
                            totalProgress += p.progress;
                        }
                        deferred.Progress(totalProgress / (float)promisesLength);
                    }
                };
            }

            return deferred.promise;
        }

        public static Promise<T> Any<T>(params Promise<T>[] promises) {
            var deferred = new Deferred<T>();
            var failed = 0;

            var initialProgress = 0f;
            foreach (var p in promises) {
                if (p.progress > initialProgress) {
                    initialProgress = p.progress;
                }
            }
            deferred.Progress(initialProgress);

            for (int i = 0, promisesLength = promises.Length; i < promisesLength; i++) {
                var localIndex = i;
                var promise = promises[localIndex];
                promise.OnFulfilled += result => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        deferred.Fulfill(result);
                    }
                };
                promise.OnFailed += error => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        Interlocked.Increment(ref failed);
                        if (failed == promisesLength) {
                            deferred.Fail(new PromiseAnyException());
                        }
                    }
                };
                promise.OnProgressed += progress => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        var maxProgress = 0f;
                        foreach (var p in promises) {
                            if (p.progress > maxProgress) {
                                maxProgress = p.progress;
                            }
                        }
                        deferred.Progress(maxProgress);
                    }
                };
            }

            return deferred.promise;
        }

        public static Promise<object[]> Collect(params Promise<object>[] promises) {
            var deferred = new Deferred<object[]>();
            var results = new object[promises.Length];
            var done = 0;

            var initialProgress = 0f;
            foreach (var p in promises) {
                initialProgress += p.progress;
            }

            deferred.Progress(initialProgress / (float)promises.Length);

            for (int i = 0, promisesLength = promises.Length; i < promisesLength; i++) {
                var localIndex = i;
                var promise = promises[localIndex];
                promise.OnFulfilled += result => {
                    results[localIndex] = result;
                    Interlocked.Increment(ref done);
                    if (done == promisesLength) {
                        deferred.Fulfill(results);
                    }
                };
                promise.OnFailed += error => {
                    var totalProgress = 0f;
                    foreach (var p in promises) {
                        totalProgress += p.state == PromiseState.Failed ? 1f : p.progress;
                    }
                    deferred.Progress(totalProgress / (float)promisesLength);
                    Interlocked.Increment(ref done);
                    if (done == promisesLength) {
                        deferred.Fulfill(results);
                    }
                };
                promise.OnProgressed += progress => {
                    var totalProgress = 0f;
                    foreach (var p in promises) {
                        totalProgress += p.state == PromiseState.Failed ? 1f : p.progress;
                    }
                    deferred.Progress(totalProgress / (float)promisesLength);
                };
            }

            return deferred.promise;
        }
    }
}
