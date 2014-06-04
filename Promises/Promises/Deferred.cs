namespace Promises {
    public class Deferred<T> : Promise<T> {
        public Promise<T> promise { get { return this; } }

        public void Fulfill(T result) {
            _result = result;
            _progress = 1f;
            transitionToState(PromiseState.Fulfilled);
        }
    }
}

