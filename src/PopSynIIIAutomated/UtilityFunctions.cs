using System;
using System.IO;

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
    /// Creates the directories for the given path and returns the original path.
    /// </summary>
    /// <param name="path">The path to ensure that the directories exist for.</param>
    /// <returns>The same string as path.</returns>
    internal static string CreateDirectories(string path)
    {
        var dirName = Path.GetDirectoryName(path);
        if (dirName is not null)
        {
            Directory.CreateDirectory(dirName);
        }
        return path;
    }
}

