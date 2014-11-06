using System;
using System.Threading;

namespace Promises {
    public class Deferred<T> : Promise<T> {
        public Promise<T> promise { get { return this; } }
        public Func<T> action;

        public Promise<T> RunAsync() {
            _thread = new Thread(() => {
                try {
                    Fulfill(action());
                } catch (Exception ex) {
                    Fail(ex);
                }
            });
            _thread.Name = "Deferred.RunAsync(" + action + ")";
            _thread.IsBackground = true;
            _thread.Start();
            return promise;
        }

        public void Fulfill(T result) {
            _result = result;
            setProgress(1f);
            transitionToState(PromiseState.Fulfilled);
        }

        public void Fail(Exception ex) {
            _error = ex;
            transitionToState(PromiseState.Failed);
        }

        public void Progress(float progress) {
            setProgress(progress);
        }
    }

    public class PromiseAnyException : Exception {
        public PromiseAnyException() : base("All promises did fail!") {
        }
    }
}

