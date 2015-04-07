using System;

namespace Promises {
    public enum PromiseState : byte {
        Unfulfilled,
        Failed,
        Fulfilled
    }

    public class State<T> {
        public PromiseState state { get { return _state; } }
        public T result { get { return _result; } }
        public Exception error { get { return _error; } }
        public float progress { get { return _progress; } }
        public bool allDelegatesCalled { get { return _allDelegatesCalled; } }

        readonly PromiseState _state;
        readonly T _result;
        readonly Exception _error;
        readonly float _progress;
        readonly bool _allDelegatesCalled;

        State(PromiseState state, T result, Exception error, float progress, bool allDelegatesCalled) {
            _state = state;
            _result = result;
            _error = error;
            _progress = progress;
            _allDelegatesCalled = allDelegatesCalled;
        }

        public static State<T> CreateUnfulfilled() {
            return new State<T>(PromiseState.Unfulfilled, default(T), null, 0f, false);
        }

        public State<T> SetFulfilled(T result) {
            return new State<T>(PromiseState.Fulfilled, result, null, 1f, false);
        }

        public State<T> SetFailed(Exception error) {
            return new State<T>(PromiseState.Failed, _result, error, _progress, false);
        }

        public State<T> SetProgress(float p) {
            return new State<T>(_state, _result, _error, p, false);
        }

        public State<T> SetAllDelegatesCalled() {
            return new State<T>(_state, _result, _error, _progress, true);
        }
    }
}
