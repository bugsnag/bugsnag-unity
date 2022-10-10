using System;
using System.Threading.Tasks;

public class AsyncException : Scenario
{
    public override void Run()
    {
        DoAsyncTest();
    }

    private async void DoAsyncTest()
    {
        throw new Exception("AsyncException");
        await Task.Yield();
    }
}
