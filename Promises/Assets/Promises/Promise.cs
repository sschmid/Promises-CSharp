using System;
using System.Collections;

namespace Promises {
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

        public PromiseState state { get { return _state.state; } }
        public T result { get { return _state.result; } }
        public Exception error { get { return _state.error; } }
        public float progress { get { return _state.progress; } }

        event Fulfilled _onFulfilled;
        event Failed _onFailed;
        event Progressed _onProgressed;

        protected volatile State<T> _state;
        readonly object _lock = new object();

        int _depth = 1;
        float _bias = 0f;
        float _fraction = 1f;

        public Promise() {
            _state = State<T>.CreateUnfulfilled();
        }

        public void Await() {
            while (state == PromiseState.Unfulfilled || !_state.allDelegatesCalled);
        }

        public Promise<TThen> Then<TThen>(Func<T, TThen> action) {
            var deferred = new Deferred<TThen>();
            deferred.action = () => action(result);
            return Then(deferred.promise);
        }

        public Promise<TThen> ThenCoroutine<TThen>(Func<T, IEnumerator> coroutine) {
            var deferred = new Deferred<TThen>();
            deferred.coroutine = () => coroutine(result);
            return Then(deferred.promise);
        }

        public Promise<TThen> Then<TThen>(Promise<TThen> promise) {
            var deferred = (Deferred<TThen>)promise;
            deferred._depth = _depth + 1;
            deferred._fraction = 1f / deferred._depth;
            deferred._bias = (float)_depth / (float)deferred._depth * progress;
            deferred.Progress(0);

            // Unity workaround. For unknown reasons, Unity won't compile using OnFulfilled += ..., OnFailed += ... or OnProgressed += ...
            addOnFulfilled(result => deferred.RunAsync());
            addOnFailed(deferred.Fail);
            addOnProgress(p => {
                deferred._bias = (float)_depth / (float)deferred._depth * p;
                deferred.Progress(0);
            });
            return deferred.promise;
        }

        public Promise<T> Rescue(Func<Exception, T> action) {
            var deferred = createDeferredRescue();
            deferred.action = () => action(error);
            return deferred.promise;
        }

        public Promise<T> RescueCoroutine(Func<Exception, IEnumerator> coroutine) {
            var deferred = createDeferredRescue();
            deferred.coroutine = () => coroutine(error);
            return deferred.promise;
        }

        Deferred<T> createDeferredRescue() {
            var deferred = new Deferred<T>();
            deferred._depth = _depth;
            deferred._fraction = 1f;
            deferred.Progress(progress);
            addOnFulfilled(deferred.Fulfill);
            addOnFailed(error => deferred.RunAsync());
            addOnProgress(deferred.Progress);
            return deferred;
        }

        public Promise<TWrap> Wrap<TWrap>() {
            var deferred = new Deferred<TWrap>();
            deferred._depth = _depth;
            deferred._fraction = 1f;
            deferred.Progress(progress);
            addOnFulfilled(result => deferred.Fulfill((TWrap)(object)result));
            addOnFailed(deferred.Fail);
            addOnProgress(deferred.Progress);
            return deferred.promise;
        }

        public override string ToString() {
            if (state == PromiseState.Fulfilled) {
                return string.Format("[Promise<{0}>: state = {1}, result = {2}]", typeof(T).Name, state, result);
            }
            if (state == PromiseState.Failed) {
                return string.Format("[Promise<{0}>: state = {1}, progress = {2:0.###}, error = {3}]", typeof(T).Name, state, progress, error.Message);
            }

            return string.Format("[Promise<{0}>: state = {1}, progress = {2:0.###}]", typeof(T).Name, state, progress);
        }

        void addOnFulfilled(Fulfilled value) {
            lock (_lock) {
                if (state == PromiseState.Unfulfilled) {
                    _onFulfilled += value;
                } else if (state == PromiseState.Fulfilled) {
                    value(result);
                }
            }
        }

        void addOnFailed(Failed value) {
            lock (_lock) {
                if (state == PromiseState.Unfulfilled) {
                    _onFailed += value;
                } else if (state == PromiseState.Failed) {
                    value(error);
                }
            }
        }

        void addOnProgress(Progressed value) {
            lock (_lock) {
                if (progress < 1f) {
                    _onProgressed += value;
                } else {
                    value(progress);
                }
            }
        }

        protected void transitionToFulfilled(T result) {
            lock (_lock) {
                if (state == PromiseState.Unfulfilled) {
                    var oldProgress = progress;
                    _state = _state.SetFulfilled(result);
                    if (progress != oldProgress) {
                        if (_onProgressed != null) {
                            _onProgressed(progress);
                        }
                    }
                    if (_onFulfilled != null) {
                        _onFulfilled(result);
                    }
                } else {
                    throw new Exception(string.Format("Invalid state transition from {0} to {1}", state, PromiseState.Fulfilled));
                }
            }

            cleanup();
        }

        protected void transitionToFailed(Exception error) {
            lock (_lock) {
                if (state == PromiseState.Unfulfilled) {
                    _state = _state.SetFailed(error);
                    if (_onFailed != null) {
                        _onFailed(error);
                    }
                } else {
                    throw new Exception(string.Format("Invalid state transition from {0} to {1}", state, PromiseState.Failed));
                }
            }
            
            cleanup();
        }

        protected void setProgress(float p) {
            lock (_lock) {
                var newProgress = _bias + p * _fraction;
                if (newProgress != progress) {
                    _state = _state.SetProgress(newProgress);
                    if (_onProgressed != null) {
                        _onProgressed(progress);
                    }
                }
            }
        }

        void cleanup() {
            _onFulfilled = null;
            _onFailed = null;
            _onProgressed = null;
            _state = _state.SetAllDelegatesCalled();
        }
    }
}
