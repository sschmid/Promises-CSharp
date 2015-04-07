using System;
using System.Collections;
using UnityEngine;

namespace Promises {
    public class Coroutine<T> {
        public T returnValue {
            get {
                if (_exception != null) {
                    throw _exception;
                }
                return _returnValue;
            }
        }

        public Coroutine coroutine;

        T _returnValue;
        Exception _exception;

        public IEnumerator WrapRoutine(IEnumerator routine) {
            while (true) {
                try {
                    if (!routine.MoveNext()) {
                        _returnValue = (T)routine.Current;
                        yield break;
                    }
                } catch (Exception ex) {
                    _exception = ex;
                    yield break;
                }

                yield return routine.Current;
            }
        }
    }
}
