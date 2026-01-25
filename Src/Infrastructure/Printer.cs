namespace Neurocommenting.Infrastructure;

public static class Printer
{
    private static object lockObject = new object();
    
    public static void PrintError(string text) => Print(text, ConsoleColor.Red);
    public static void PrintSuccess(string text) => Print(text, ConsoleColor.Green);
    public static void PrintInfo(string text) => Print(text, ConsoleColor.Blue);
    public static void PrintWarning(string text) => Print(text, ConsoleColor.Yellow);
    
    public static void Print(string text, ConsoleColor color)
    {
        lock (lockObject)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"-> {text}");
            Console.ResetColor();
        }
    }
}