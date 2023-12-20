using Log.Data;
using System;

namespace ApplicationCore;
internal class Program {
    //entry-point of the application
    public static void Main() {
        using ApplicationManager applicationManager = ApplicationManager.Instance; //get the application manager
        applicationManager.LogManager.ReceivedEntry += ConsoleWriter; //add the event listener
        applicationManager.Run(); //run the application
    }

    //handles the incomming logs and writes them to the console
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
