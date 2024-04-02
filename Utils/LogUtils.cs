namespace RoundEndSound.Utils;

public class LogUtils
{
    public void Log(string? message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("[Round End Sound] " + message);
        Console.ResetColor();
    }
}