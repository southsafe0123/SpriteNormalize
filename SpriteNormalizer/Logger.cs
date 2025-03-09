using System;

public static class Logger
{
    public static void Log(string s)
    {
        Console.WriteLine($"Log - {s}");
        Console.WriteLine();
    }
    public static void Success(string s)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Success - {s}");
        Console.WriteLine();
        Console.ResetColor();
    }
    public static void Error(string s)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error - {s}");
        Console.WriteLine();
        Console.ResetColor();
    }
    public static void Warning(string s)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning - {s}");
        Console.WriteLine();
        Console.ResetColor();
    }
}