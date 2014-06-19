using System;
using System.Threading;

namespace Promises {
    public enum PromiseState : byte {
        Unfulfilled,
        Failed,
        Fulfilled
    }

    public class Promise<TResult> {
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

        public delegate void Fulfilled(TResult result);
        public delegate void Failed(Exception error);
        public delegate void Progressed(float progress);
        public PromiseState state { get { return _state; } }
        public TResult result { get { return _result; } }
        public Exception error { get { return _error; } }
        public float progress { get { return _progress; } }
        public Thread thread { get { return _thread; } }

        event Fulfilled _onFulfilled;
        event Failed _onFailed;
        event Progressed _onProgressed;

        protected PromiseState _state;
        protected TResult _result;
        protected Exception _error;
        protected float _progress;
        protected Thread _thread;
        protected int _depth = 1;

        public static Promise<TResult> PromiseWithAction(Func<TResult> action) {
            return new Deferred<TResult>().RunAsync(action);
        }

        public Promise<TThenResult> Then<TThenResult>(Func<TResult, TThenResult> action) {
            var deferred = new Deferred<TThenResult>();
            deferred._depth = _depth + 1;
            // Unity workaround. Unity won't compile using OnFulfilled += ..., OnFailed += ... or OnProgressed += ...
            addOnFulfilled(result => deferred.RunAsync(() => action(result)));
            addOnFailed(deferred.Fail);
            addOnProgress(progress => deferred.setProgress(progress / deferred._depth * _depth));
            return deferred.promise;
        }

        public Promise<TResult> Rescue(Func<Exception, TResult> action) {
            var deferred = new Deferred<TResult>();
            deferred._depth = _depth;
            addOnFulfilled(deferred.Fulfill);
            addOnFailed(error => deferred.RunAsync(() => action(error)));
            addOnProgress(deferred.setProgress);
            return deferred.promise;
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
            _progress = progress;
            if (_onProgressed != null)
                _onProgressed(_progress);
        }

        void cleanup() {
            _onFulfilled = null;
            _onFailed = null;
            _onProgressed = null;
            _thread = null;
        }
    }
}

