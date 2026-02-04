namespace Neurocommenting.Infrastructure;

public static class Logger
{
    private static StreamWriter logWriter = new StreamWriter(AppPaths.LogFile(DateTime.Now), append: true)
    {
        AutoFlush = true
    };
    
    public static void Write(int level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        logWriter.WriteLine($"[{timestamp}] [{level}] {message}");
    }
}