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
            ThreadPool.QueueUserWorkItem(state => {

                T actionResult = default(T);
                bool fulfilled = false;

                try {
                    actionResult = action();
                    fulfilled = true;
                } catch (Exception ex) {
                    Fail(ex);
                }

                if (fulfilled) {
                    Fulfill(actionResult);
                }
            });
        }

        void runCoroutine() {
            MainThreadDispatcher.Dispatch(() => CoroutineRunner.StartRoutine<T>(coroutine(), c => {

                T actionResult = default(T);
                bool fulfilled = false;

                try {
                    actionResult = c.returnValue;
                    fulfilled = true;
                } catch (Exception ex) {
                    Fail(ex);
                }

                if (fulfilled) {
                    Fulfill(actionResult);
                }
            }));
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
