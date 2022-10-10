using System;

public class InnerException : Scenario
{
    public override void Run()
    {
        throw new Exception("Outer", new Exception("Inner"));
    }
}
