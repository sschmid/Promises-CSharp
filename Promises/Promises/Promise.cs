using System;
using System.Threading;

namespace Promises {
    public enum PromiseState : byte {
        Unfulfilled,
        Failed,
        Fulfilled
    }

    public class Promise<T> {
        public event StateChange OnStateChanged {
            add {
                _onStateChanged += value;
                if (_state != PromiseState.Unfulfilled)
                    value(this);
            }
            remove {
                _onStateChanged -= value;
            }
        }

        public delegate void StateChange(Promise<T> promise);

        public PromiseState state { get { return _state; } }

        public T result { get { return _result; } }

        public float progress { get { return _progress; } }

        event StateChange _onStateChanged;

        protected PromiseState _state;
        protected T _result;
        protected float _progress;

        public static Promise<T> PromiseWithAction(Func<T> action) {
            var deferred = new Deferred<T>();
            ThreadPool.QueueUserWorkItem(state => deferred.Fulfill(action()));
            return deferred.promise;
        }

        protected void transitionToState(PromiseState newState) {
            if (_state == PromiseState.Unfulfilled) {
                _state = newState;
                if (_onStateChanged != null)
                    _onStateChanged(this);
            } else {
                throw new Exception(string.Format("Invalid state transition from {0} to {1}", _state, newState));
            }
        }
    }
}

