using System.Threading;

public static class TestHelper {
    public static int CheapAction() {
        return Thread.CurrentThread.ManagedThreadId;
    }

    public static int ExpensiveAction() {
        var a = 0;
        for (int i = 0; i < 5000000; i++)
            a++;
        return Thread.CurrentThread.ManagedThreadId;
    }

}

