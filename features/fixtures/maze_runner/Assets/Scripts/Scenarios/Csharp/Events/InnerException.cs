using System;

public class InnerException : Scenario
{
    public override void Run()
    {
        throw new Exception("InnerException Outer", new Exception("InnerException Inner"));
    }
}
