using Promises;
using System;
using System.Threading;

public static class TestHelper {
    public static Promise<T> PromiseWithResult<T>(T result, int delay = 0) {
        return Promise<T>.PromiseWithAction(() => {
            if (delay > 0)
                Thread.Sleep(delay);
            return result;
        });
    }

    public static Promise<T> PromiseWithError<T>(string errorMessage, int delay = 0) {
        return Promise<T>.PromiseWithAction(() => {
            if (delay > 0)
                Thread.Sleep(delay);
            throw new Exception(errorMessage);
        });
    }
}

