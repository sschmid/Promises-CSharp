using System;
using System.Threading;

namespace Promises {
    public class Deferred<TResult> : Promise<TResult> {
        public Promise<TResult> promise { get { return this; } }

        public Promise<TResult> RunAsync(Func<TResult> action) {
            _thread = new Thread(() => {
                try {
                    Fulfill(action());
                } catch (Exception ex) {
                    Fail(ex);
                }
            });
            _thread.IsBackground = true;
            _thread.Start();
            return promise;
        }

        public void Fulfill(TResult result) {
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
}

