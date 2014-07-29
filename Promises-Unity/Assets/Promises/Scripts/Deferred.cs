using System;
using System.Collections;
using System.Threading;

namespace Promises {
    public class Deferred<T> : Promise<T> {
        public Promise<T> promise { get { return this; } }
        public Func<T> action;
		public IEnumerator coroutine;

        public Promise<T> RunAsync() {
			if (coroutine != null) {
				RunAsync(coroutine);
			}

			else {
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
            return promise;
        }

		public Promise<T> RunAsync(IEnumerator coroutine) {
			CoroutineRunner.StartRoutine<T>(coroutine, HandleCallback);
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

		void HandleCallback (Coroutine<T> coroutine) {
			try {
				Fulfill(coroutine.Value);
			} catch (Exception ex) {
				Fail(ex);
			}
		}
    }

    public class PromiseAnyException : Exception {
        public PromiseAnyException() : base("All promises did fail!") {
        }
    }
}

