using Log.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ConsoleApp;

/// <summary>
/// manages writing the log file
/// </summary>
internal class LogFileManager {
    private const string LOG_NAME = "latest.log";    //the name of the log file, set to 'latest.log'

    private readonly string _latestLogPath;         //the path where the 'latest.log' file is located
    private DateTime _logDate = DateTime.MaxValue;  //the date of 'latest.log', if the current date surpasses this 'latest.log' is compressed, initial value is max so 'latest.log' is always compressed at start
    private FileStream _logStream;                  //the fileStream of the 'latest.log' file
    private StreamWriter _streamWriter;             //the stream writer of 'latest.log'

    public LogFileManager(string logDirectory) {
        _latestLogPath = Path.Combine(logDirectory, LOG_NAME);

        //compress if the latest log file already exists
        if (File.Exists(_latestLogPath))
            Compress();

        //create a new streamWriter
        _logStream = NewFileStream();
        _streamWriter = new StreamWriter(_logStream);
    }

    private string LogDirectory {
        get => Path.GetDirectoryName(_latestLogPath) ?? throw new NullReferenceException();
    }

    #region writing to file
    /// <summary>
    /// writes <paramref name="entry"/> to the log file
    /// </summary>
    public void WriteToFile(LogEntry entry) {
        //compress if the date that the log file has been written is smaller than the current date
        if (_logDate < DateTime.Now.Date) {
            Console.ResetColor();
            Console.Clear();

            //close the streamWriters
            _streamWriter.Close();
            _logStream.Close();

            //compress the current file
            Compress();

            //create a new stream
            _logStream = NewFileStream();
            _streamWriter = new StreamWriter(_logStream);
        }

        //set the log date to the date right now
        _logDate = DateTime.Now.Date;

        //write to the file
        _streamWriter.Write(entry.ToString() + _streamWriter.NewLine);
        _streamWriter.Flush();
    }
    #endregion

    /// <returns>
    /// creates a new file (or overrides the old one) and returns it as stream writer
    /// </returns>
    private FileStream NewFileStream() {
        Directory.CreateDirectory(LogDirectory);
        //create the file and create the streamWriter
        return new FileStream(_latestLogPath, FileMode.Create, FileAccess.Write, FileShare.Read);
    }

    #region compressing file
    /// <summary>
    /// compresses the current log file and clears it
    /// </summary>
    private void Compress() {
        string date;
        string logArchivePath;
        string suffix;
        int pathCount;
        IEnumerable<string> currentLogPaths;

        //get the creation date and format it to a string
        date = File.GetLastWriteTime(_latestLogPath).ToString("dd-MM-yyyy");

        //use a query to find all the log files of today
        currentLogPaths =
            from filePath in Directory.GetFiles(LogDirectory)
            where Path.GetFileName(filePath).StartsWith(date)
            select filePath;

        //get the correct suffix
        pathCount = currentLogPaths.Count(); //get the count of the results of the query
        suffix = pathCount != 0 ? $"_{pathCount}" : string.Empty; //if the pathCount isn't 0, set the suffix

        //create the log archive path
        logArchivePath = Path.Combine(LogDirectory, $"{date}{suffix}.log.gz");

        //create the archive
        using FileStream readStream = File.OpenRead(_latestLogPath); //open the file in read mode
        using FileStream createSteam = File.Create(logArchivePath); //create a new file at the path of the archive
        using GZipStream gZipStream = new(createSteam, CompressionLevel.Fastest, false); //create the gZip stream
        readStream.CopyTo(gZipStream); //copy the read stream to the gZipStream
    }
    #endregion
}
