using ConsoleApp;
using Log.Data;
using System;

namespace ApplicationCore;
internal class Program {
    //entry-point of the application
    public static void Main() {
        //init console
        Console.ResetColor(); //reset console colour
        Console.Clear(); //clear the console
        Console.CursorVisible = false; //set the cursor visibility to false
        Console.In.Dispose(); //dispose the input stream as we won't need it
        LogFileManager logFileManager = new("./logs");

        //
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
