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
            var t = new Thread(() => {
                try {
                    Fulfill(action());
                } catch (Exception ex) {
                    Fail(ex);
                }
            });
            t.Name = "Deferred.RunAsync(" + action + ")";
            t.IsBackground = true;

            _state = _state.SetThread(t);

            t.Start();
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
            transitionToFulfilled(result);
        }

        public void Fail(Exception ex) {
            transitionToFailed(ex);
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

