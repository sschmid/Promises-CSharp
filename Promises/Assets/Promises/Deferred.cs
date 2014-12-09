using System;
using System.Collections;
using System.Threading;

namespace Promises {
    public class Deferred<T> : Promise<T> {
        public Promise<T> promise { get { return this; } }

        public Func<T> action;
        public Func<IEnumerator> coroutine;

        public Promise<T> RunAsync() {
            if (coroutine == null) {
                runAction();
            } else {
                runCoroutine();
            }

            return promise;
        }

        void runAction() {
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
        }

        void runCoroutine() {
            CoroutineRunner.StartRoutine<T>(coroutine(), c => {
                try {
                    Fulfill(c.returnValue);
                } catch (Exception ex) {
                    Fail(ex);
                }
            });
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

