using UnityEngine;
using Promises;
using System;

public static class PromiseWrapperExtension {
    public static PromiseWrapper QueueOnMainThread<T>(this Promise<T> promise) {
        return PromiseWrapper.Wrap(promise);
    }
}

public class PromiseWrapper : MonoBehaviour {

    public event Fulfilled OnFulfilled {
        add { addOnFulfilled(value); }
        remove { _onFulfilled -= value; }
    }

    public event Failed OnFailed {
        add { addOnFailed(value); }
        remove { _onFailed -= value; }
    }

    public event Progressed OnProgressed {
        add { addOnProgress(value); }
        remove { _onProgressed -= value; }
    }

    public delegate void Fulfilled(object result);
    public delegate void Failed(Exception error);
    public delegate void Progressed(float progress);

    event Fulfilled _onFulfilled;
    event Failed _onFailed;
    event Progressed _onProgressed;

    Promise<object> _promise;
    object _result;
    Exception _error;
    float _progress;
    bool _updateProgress;

    public static PromiseWrapper Wrap<T>(Promise<T> promise, string name = "Promise") {
        var wrapper = new GameObject(name).AddComponent<PromiseWrapper>();
        wrapper.init(promise.Wrap<object>());
        return wrapper;
    }

    void init(Promise<object> promise) {
        _promise = promise;
        promise.OnFulfilled += result => _result = result;
        promise.OnFailed += error => _error = error;
        promise.OnProgressed += progress => {
            _progress = progress;
            _updateProgress = true;
        };
    }

    void addOnFulfilled(Fulfilled value) {
        if (_promise.state == PromiseState.Unfulfilled)
            _onFulfilled += value;
        else if (_promise.state == PromiseState.Fulfilled)
            value(_result);
    }

    void addOnFailed(Failed value) {
        if (_promise.state == PromiseState.Unfulfilled)
            _onFailed += value;
        else if (_promise.state == PromiseState.Failed)
            value(_error);
    }

    void addOnProgress(Progressed value) {
        if (_progress < 1f)
            _onProgressed += value;
        else
            value(_progress);
    }

    void Update() {
        if (_updateProgress) {
            _updateProgress = false;
            if (_onProgressed != null)
                _onProgressed(_progress);
        }
        if (_result != null) {
            if (_onFulfilled != null)
                _onFulfilled(_result);
            cleanup();
        }
        if (_error != null) {
            if (_onFailed != null)
                _onFailed(_error);
            cleanup();
        }
    }

    void cleanup() {
        _onFulfilled = null;
        _onFailed = null;
        _onProgressed = null;
        Destroy(gameObject);
    }
}
