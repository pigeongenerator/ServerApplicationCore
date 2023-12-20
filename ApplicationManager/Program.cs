using Log.Data;
using System;

namespace ApplicationCore;
internal class Program {
    public static void Main() {
        using ApplicationManager applicationManager = ApplicationManager.Instance;
        applicationManager.LogManager.ReceivedEntry += ConsoleWriter;
        applicationManager.Run();
    }

    private static void ConsoleWriter(LogEntry entry) {
        ConsoleColor original = Console.ForegroundColor;
        Console.ForegroundColor = entry.Severity switch {
            LogSeverity.Info => original,
            LogSeverity.Debug => ConsoleColor.Magenta,
            LogSeverity.Warn => ConsoleColor.DarkYellow,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Fatal => ConsoleColor.DarkRed,
            _ => original,
        };

        Console.WriteLine(entry);
        Console.ForegroundColor = original;
    }
}
