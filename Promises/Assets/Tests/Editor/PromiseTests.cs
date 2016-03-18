using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Promises;

[TestFixture]
class PromiseTests {

    void sleepShort() {
        Thread.Sleep(5);
    }

    void sleepLong() {
        Thread.Sleep(10);
    }

    [Test]
    public void QueueOnMainThread_returns_promise() {
        var p = Promise.WithAction(() => 42);
        Assert.AreSame(p, p.QueueOnMainThread(null));
    }

    [Test]
    public void fulfilled_promise_calls_onFulfilled_immediately() {
        var p = Promise.WithAction(() => 42);
        sleepShort();
        var result = 0;
        p.QueueOnMainThread(r => result = r);
        Assert.AreEqual(42, result);
    }

    [Test]
    public void handles_null_onFulfilled_hanlder() {
        var p = Promise.WithAction(() => 42);
        sleepShort();
        p.QueueOnMainThread(null);
    }

    [Test]
    public void failed_promise_calls_onFailed_immediately() {
        var p = Promise.WithAction<int>(() => {
            throw new Exception("42");
        });
        sleepShort();
        var errorMsg = "";
        p.QueueOnMainThread(null, error => errorMsg = error.Message);
        Assert.AreEqual("42", errorMsg);
    }

    [Test]
    public void handles_null_onFailed_hanlder() {
        var p = Promise.WithAction<int>(() => {
            throw new Exception("42");
        });
        sleepShort();
        p.QueueOnMainThread(null, null);
    }

    [Test]
    public void calls_onProgressed_immediately_when_greater_0() {
        var d = new Deferred<int>();
        d.Progress(0.42f);
        var progress = 0f;
        d.promise.QueueOnMainThread(null, null, p => {
            progress = p;
        });
        Assert.AreEqual(0.42f, progress);
    }

    [Test]
    public void does_not_call_onProgressed_immediately_when_0() {
        var d = new Deferred<int>();
        d.Progress(0f);
        var progressCalled = false;
        d.promise.QueueOnMainThread(null, null, p => {
            progressCalled = true;
        });
        Assert.IsFalse(progressCalled);
    }

    [Test]
    public void handles_null_onProgressed_hanlder() {
        var d = new Deferred<int>();
        d.Progress(0.42f);
        d.QueueOnMainThread(null, null, null);
    }

    [Test]
    public void then_coroutine_after_action() {
        MainThreadDispatcher.Init();
        var p = Promise
            .WithAction(() => 42)
            .ThenCoroutine<int>(coroutine);

        p.OnFulfilled += result => Assert.AreEqual(42 * 42, result);
        p.OnFailed += error => Assert.Fail();
    }

    [Test]
    public void when_OnFulfilled_throws() {
        MainThreadDispatcher.Init();
        var p = Promise.WithCoroutine<int>(() => coroutine(42));

        p.OnFulfilled += result => {
            throw new IndexOutOfRangeException("Ignore me - I'm here intentionally");
        };
        p.OnFailed += error => Assert.Fail();
    }

    [Test, ExpectedException(typeof(IndexOutOfRangeException))]
    public void when_OnFailed_throws() {
        MainThreadDispatcher.Init();
        var p = Promise.WithCoroutine<int>(() => coroutineWithError());

        p.OnFulfilled += result => Assert.Fail();
        p.OnFailed += error => Assert.Fail();
    }

    System.Collections.IEnumerator coroutine(int result) {
        yield return 42 * result;
    }

    System.Collections.IEnumerator coroutineWithError() {
        throw new IndexOutOfRangeException("Ignore me - I'm here intentionally");
    }
}

