using System.Reflection;

public static class MarseyLogger
{
    private enum LogType
    {
        INFO,
        WARN,
        FATL,
        DEBG
    }
    
    public delegate void Forward(AssemblyName asm, string message);
    
    public static Forward? logDelegate;

    private static void Log(LogType type, string message)
    {
        logDelegate?.Invoke(Assembly.GetExecutingAssembly().GetName(), $"[{type.ToString()}] {message}");
    }

    public static void Info(string message)
    {
        Log(LogType.INFO, message);
    }
    
    public static void Warn(string message)
    {
        Log(LogType.WARN, message);
    }
    
    public static void Fatal(string message)
    {
        Log(LogType.FATL, message);
    }
    
    public static void Debug(string message)
    {
        Log(LogType.DEBG, message);
    }
}