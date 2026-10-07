namespace HoboXslt.Core.Tests;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is [Worker.Argument])
            Worker.Serve();
    }
}
