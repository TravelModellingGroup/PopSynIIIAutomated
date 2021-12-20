using System;
using System.IO;
using System.Text;

namespace PopSynIIIAutomatedTest;

/// <summary>
/// This class provides utility functions that will be used in the test methods.
/// </summary>
internal static class Utilities
{
    /// <summary>
    /// Returns a string of what was written by the stream writer.
    /// </summary>
    /// <param name="toTest">The function to execute that requires a stream writer.</param>
    /// <returns>The string representation of what the toTest function wrote to the writer.</returns>
    internal static string GetStreamResult(Action<StreamWriter> toTest)
    {
        using var memory = new MemoryStream();
        using var writer = new StreamWriter(memory, Encoding.Default, leaveOpen: true);
        toTest(writer);
        writer.Flush();
        return Encoding.Default.GetString(memory.ToArray());
    }

    /// <summary>
    /// Try to delete the file given the path.
    /// </summary>
    /// <param name="filePath">The path to try to delete.</param>
    internal static void DeleteIfFileExists(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch
        {
        }
    }
}
