using System;

public static class Logger
{
    public static void Log(string s)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Log - {s}");
        Console.ResetColor();
    }
    public static void Success(string s)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Success - {s}");
        Console.ResetColor();
    }
    public static void Error(string s)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error - {s}");
        Console.ResetColor();
    }
    public static void Warning(string s)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning - {s}");
        Console.ResetColor();
    }
    public static void Announce(string s)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Announce - {s}");
        Console.ResetColor();
    }
}