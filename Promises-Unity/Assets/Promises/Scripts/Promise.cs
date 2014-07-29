using System;
using System.Collections;
using System.Threading;

namespace Promises {
    public enum PromiseState : byte {
        Unfulfilled,
        Failed,
        Fulfilled
    }

	public static class Promise {
		public static Promise<T> WithAction<T>(Func<T> action) {
			var deferred = new Deferred<T>();
			deferred.action = action;
			return deferred.RunAsync();
		}

		public static Promise<T> WithCoroutine<T> (IEnumerator coroutine) {
			var deferred = new Deferred<T>();
			return deferred.RunAsync(coroutine);
		}

		public static Promise<object[]> All(params Promise<object>[] promises) {
            var deferred = new Deferred<object[]>();
            var results = new object[promises.Length];
            var done = 0;

            var initialProgress = 0f;
            foreach (var p in promises)
                initialProgress += p.progress;
            deferred.Progress(initialProgress / (float)promises.Length);

            for (int i = 0, promisesLength = promises.Length; i < promisesLength; i++) {
                var localIndex = i;
                var promise = promises[localIndex];
                promise.OnFulfilled += result => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        results[localIndex] = result;
                        if (++done == promisesLength)
                            deferred.Fulfill(results);
                    }
                };
                promise.OnFailed += error => {
                    if (deferred.state == PromiseState.Unfulfilled)
                        deferred.Fail(error);
                };
                promise.OnProgressed += progress => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        var totalProgress = 0f;
                        foreach (var p in promises)
                            totalProgress += p.progress;
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
            foreach (var p in promises)
                if (p.progress > initialProgress)
                    initialProgress = p.progress;
            deferred.Progress(initialProgress);

            for (int i = 0, promisesLength = promises.Length; i < promisesLength; i++) {
                var localIndex = i;
                var promise = promises[localIndex];
                promise.OnFulfilled += result => {
                    if (deferred.state == PromiseState.Unfulfilled)
                        deferred.Fulfill(result);
                };
                promise.OnFailed += error => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        if (++failed == promisesLength)
                            deferred.Fail(new PromiseAnyException());
                    }
                };
                promise.OnProgressed += progress => {
                    if (deferred.state == PromiseState.Unfulfilled) {
                        var maxProgress = 0f;
                        foreach (var p in promises)
                            if (p.progress > maxProgress)
                                maxProgress = p.progress;
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
            foreach (var p in promises)
                initialProgress += p.progress;
            deferred.Progress(initialProgress / (float)promises.Length);

            for (int i = 0, promisesLength = promises.Length; i < promisesLength; i++) {
                var localIndex = i;
                var promise = promises[localIndex];
                promise.OnFulfilled += result => {
                    results[localIndex] = result;
                    if (++done == promisesLength)
                        deferred.Fulfill(results);
                };
                promise.OnFailed += error => {
                    var totalProgress = 0f;
                    foreach (var p in promises)
                        totalProgress += p.state == PromiseState.Failed ? 1f : p.progress;
                    deferred.Progress(totalProgress / (float)promisesLength);
                    if (++done == promisesLength)
                        deferred.Fulfill(results);
                };
                promise.OnProgressed += progress => {
                    var totalProgress = 0f;
                    foreach (var p in promises)
                        totalProgress += p.progress;
                    deferred.Progress(totalProgress / (float)promisesLength);
                };
            }

            return deferred.promise;
        }
    }

    public class Promise<T> {
        public event Fulfilled OnFulfilled {
            add { addOnFulfilled(value); }
            remove { _onFulfilled -= value; }
        }

        public event Failed OnFailed {
            add { addOnFailed(value); }
            remove { _onFailed -= value; }
        }

        public event Progressed OnProgressed {
            add { addOnProgress(value); }
            remove { _onProgressed -= value; }
        }

        public delegate void Fulfilled(T result);
        public delegate void Failed(Exception error);
        public delegate void Progressed(float progress);

        public PromiseState state { get { return _state; } }
        public T result { get { return _result; } }
        public Exception error { get { return _error; } }
        public float progress { get { return _progress; } }
        public Thread thread { get { return _thread; } }

        event Fulfilled _onFulfilled;
        event Failed _onFailed;
        event Progressed _onProgressed;

        protected PromiseState _state;
        protected T _result;
        protected Exception _error;
        protected float _progress;
        protected Thread _thread;

        int _depth = 1;
        float _bias = 0f;
        float _fraction = 1f;

        public void Await() {
            while (_state == PromiseState.Unfulfilled || _thread != null);
        }

        public Promise<TThen> Then<TThen>(Func<T, TThen> action) {
            var deferred = new Deferred<TThen>();
            deferred.action = () => action(result);
            return Then(deferred.promise);
        }

        public Promise<TThen> Then<TThen>(Promise<TThen> promise) {
            var deferred = (Deferred<TThen>)promise;
            deferred._depth = _depth + 1;
            deferred._fraction = 1f / deferred._depth;
            deferred._bias = (float)_depth / (float)deferred._depth * _progress;
            deferred.Progress(0);

            // Unity workaround. For unknown reasons, Unity won't compile using OnFulfilled += ..., OnFailed += ... or OnProgressed += ...
            addOnFulfilled(result => deferred.RunAsync());
            addOnFailed(deferred.Fail);
            addOnProgress(progress => {
                deferred._bias = (float)_depth / (float)deferred._depth * progress;
                deferred.Progress(0);
            });
            return deferred.promise;
        }

        public Promise<T> Rescue(Func<Exception, T> action) {
            var deferred = new Deferred<T>();
            deferred.action = () => action(error);
            deferred._depth = _depth;
            deferred._fraction = 1f;
            deferred.Progress(_progress);
            addOnFulfilled(deferred.Fulfill);
            addOnFailed(error => deferred.RunAsync());
            addOnProgress(deferred.Progress);
            return deferred.promise;
        }

        public Promise<TWrap> Wrap<TWrap>() {
            var deferred = new Deferred<TWrap>();
            deferred._depth = _depth;
            deferred._fraction = 1f;
            deferred.Progress(_progress);
            addOnFulfilled(result => deferred.Fulfill((TWrap)(object)result));
            addOnFailed(deferred.Fail);
            addOnProgress(deferred.Progress);
            return deferred.promise;
        }

        public override string ToString() {
            if (_state == PromiseState.Fulfilled)
                return string.Format("[Promise<{0}>: state = {1}, result = {2}]", typeof(T).Name, _state, _result);
            if (_state == PromiseState.Failed)
                return string.Format("[Promise<{0}>: state = {1}, progress = {2:0.###}, error = {3}]", typeof(T).Name, _state, _progress, error.Message);

            return string.Format("[Promise<{0}>: state = {1}, progress = {2:0.###}]", typeof(T).Name, _state, _progress);
        }

        void addOnFulfilled(Fulfilled value) {
            if (_state == PromiseState.Unfulfilled)
                _onFulfilled += value;
            else if (_state == PromiseState.Fulfilled)
                value(_result);
        }

        void addOnFailed(Failed value) {
            if (_state == PromiseState.Unfulfilled)
                _onFailed += value;
            else if (_state == PromiseState.Failed)
                value(_error);
        }

        void addOnProgress(Progressed value) {
            if (_progress < 1f)
                _onProgressed += value;
            else
                value(_progress);
        }

        protected void transitionToState(PromiseState newState) {
            if (_state == PromiseState.Unfulfilled) {
                _state = newState;
                if (_state == PromiseState.Fulfilled) {
                    if (_onFulfilled != null)
                        _onFulfilled(_result);
                } else if (_state == PromiseState.Failed) {
                    if (_onFailed != null)
                        _onFailed(_error);
                }
            } else {
                throw new Exception(string.Format("Invalid state transition from {0} to {1}", _state, newState));
            }

            cleanup();
        }

        protected void setProgress(float progress) {
            var newProgress = _bias + progress * _fraction;
            if (Math.Abs(newProgress - _progress) > float.Epsilon) {
                _progress = newProgress;
                if (_onProgressed != null)
                    _onProgressed(_progress);
            }
        }

        void cleanup() {
            _onFulfilled = null;
            _onFailed = null;
            _onProgressed = null;
            _thread = null;
        }
    }
}

