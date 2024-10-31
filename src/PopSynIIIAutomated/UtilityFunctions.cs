using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PopSynIIIAutomated;

/// <summary>
/// This class provides helper functions for the rest of the assembly.
/// </summary>
internal static class UtilityFunctions
{
    /// <summary>
    /// Write the value to the stream followed by a comma.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="value">The value to write.</param>
    internal static void WriteThenComma(StreamWriter writer, int value)
    {
        writer.Write(value);
        writer.Write(',');
    }

    /// <summary>
    /// Write the value to the stream followed by a comma.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="value">The value to write.</param>
    internal static void WriteThenComma(StreamWriter writer, float value)
    {
        writer.Write(value);
        writer.Write(',');
    }

    /// <summary>
    /// Write the value to the stream followed by a comma.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="value">The value to write.</param>
    internal static void WriteThenComma(StreamWriter writer, char value)
    {
        writer.Write(value);
        writer.Write(',');
    }

    /// <summary>
    /// Write the scaled value to the stream followed by a comma.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="value">The initial value before scaling.</param>
    /// <param name="scaleFactor">The factor to scale the value by.</param>
    internal static void WriteThenComma(StreamWriter writer, int value, float scaleFactor)
    {
        writer.Write(Scale(value, scaleFactor));
        writer.Write(',');
    }

    /// <summary>
    /// Write the scaled value to the stream followed by a comma.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="value">The initial value before scaling.</param>
    /// <param name="scaleFactor">The factor to scale the value by.</param>
    internal static void WriteThenComma(StreamWriter writer, float value, float scaleFactor)
    {
        writer.Write(Scale(value, scaleFactor));
        writer.Write(',');
    }

    /// <summary>
    /// Scale the value by the scaling factor.
    /// </summary>
    /// <param name="value">The value to scale.</param>
    /// <param name="scaleFactor">The factor to scale it by.</param>
    /// <returns>The closest integer to the scaled value.</returns>
    internal static int Scale(int value, float scaleFactor)
    {
        // We don't need to worry about the accuracy of double because
        // it can represent all 32bit integers perfectly.
        return (int)Math.Round(value * scaleFactor);
    }

    /// <summary>
    /// Scale the value by the scaling factor.
    /// </summary>
    /// <param name="value">The value to scale.</param>
    /// <param name="scaleFactor">The factor to scale it by.</param>
    /// <returns>The closest integer to the scaled value.</returns>
    internal static int Scale(float value, float scaleFactor)
    {
        // We don't need to worry about the accuracy of double because
        // it can represent all 32bit integers perfectly.
        return (int)Math.Round(value * scaleFactor);
    }

    /// <summary>
    /// Creates the directories for the given path and returns the original path.
    /// </summary>
    /// <param name="path">The path to ensure that the directories exist for.</param>
    /// <returns>The same string as path.</returns>
    internal static string CreateDirectories(string path)
    {
        var dirName = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
        return path;
    }

    /// <summary>
    /// Add quotes around the provided path.
    /// </summary>
    /// <param name="path">The path to add quotes around.</param>
    /// <returns>The path with quotes around it.</returns>
    internal static string AddQuotes(string path)
    {
        return string.Concat("\"", path, "\"");
    }

    /// <summary>
    /// Break apart a stream reader's input into a stream of lines.
    /// </summary>
    /// <param name="reader">The reader to stream from.</param>
    /// <returns>A stream of strings, one for each line of text in the StreamReader.</returns>
    internal static IEnumerable<string> StreamLines(StreamReader reader)
    {
        string? line;
        while((line = reader.ReadLine()) is not null)
        {
            yield return line;
        }
    }

    internal static int RunProcess(string programName, string cwd, string arguments, string? addToPath)
    {
        static void ReadFromStream(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line is not null)
                {
                    Runtime.WriteToUser(line);
                }
            }
        }

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(programName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = cwd,
            }
        };

        if (addToPath is not null)
        {
            process.StartInfo.EnvironmentVariables["PATH"] += ";" + addToPath;
        }
        process.Start();
        Parallel.Invoke(
            () =>
            {
                ReadFromStream(process.StandardOutput);
            }, () =>
            {
                ReadFromStream(process.StandardError);
            });
        process.WaitForExit();
        return process.ExitCode;
    }
}
