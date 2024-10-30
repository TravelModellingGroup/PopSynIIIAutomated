using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PopSynIIIAutomated;

internal static class MYSQL
{
    /// <summary>
    /// Execute a SQL command in mySQL
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <param name="sqlCommand">The command to execute.</param>
    /// <returns>If the operation returned that it completed successfully.</returns>
    public static bool ExecuteCommand(Configuration config, string sqlCommand)
    {
        try
        {
            var arguments = $"--user={config.DatabaseUsername} --password={config.DatabasePassword} {config.DatabaseName} -e \"SET sql_mode=''; {sqlCommand}\"";
            (var errorCode, string output) = RunProcess("mysql", arguments);
            if(errorCode != 0)
            {
                Runtime.WriteToUser("Failed command's output:\n" + output);
            }
            return errorCode == 0;
        }
        catch (Exception ex)
        {
            Runtime.WriteToUser(sqlCommand + "\n" + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Execute a sql script in MySQL
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <param name="scriptPath">The full path to the script file.</param>
    /// <param name="message">An optional message to send to the use before running this script.</param>
    /// <returns>If the operation completed successfully.</returns>
    public static bool ExecuteScript(Configuration config, string scriptPath, string? message = null)
    {
        if(message is not null)
        {
            Runtime.WriteToUser(message);
        }
        try
        {
            var arguments = $"--user={config.DatabaseUsername} --password={config.DatabasePassword} {config.DatabaseName} -e \"source {scriptPath}\"";
            (var errorCode, string output) = RunProcess("mysql", arguments);
            if (errorCode != 0)
            {
                Runtime.WriteToUser("Failed command's output:\n" + output);
            }
            return errorCode == 0;
        }
        catch (Exception ex)
        {
            Runtime.WriteToUser(scriptPath + "\n" + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Load the given file into the table with the specified name.
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <param name="tableName">The name of the table to write to.</param>
    /// <param name="fileName">The name of the file to read from.</param>
    /// <returns>True if the operation completed successfully.</returns>
    public static bool LoadTableFromFile(Configuration config, string tableName, string fileName)
    {
        return ExecuteCommand(config, $"LOAD DATA INFILE '{fileName.Replace("\\", "\\\\")}' INTO TABLE {tableName} FIELDS TERMINATED BY ',' LINES TERMINATED BY '\\n' IGNORE 1 LINES;");
    }

    /// <summary>
    /// Export the given table to the provided file name.
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <param name="tableName">The name of the table to export.</param>
    /// <param name="fileName">The path of the file to save to.</param>
    /// <returns>True if the operation succeeds.</returns>
    public static bool ExportTable(Configuration config, string tableName, string fileName)
    {
        // We need to delete the file before executing this command
        DeleteIfExists(fileName);
        var command = $"SELECT * INTO OUTFILE '{fileName.Replace("\\", "\\\\")}' FIELDS TERMINATED BY ',' FROM {tableName};";
        Runtime.WriteToUser(command);
        return ExecuteCommand(config, command);
    }

    /// <summary>
    /// Delete the file path if this exists
    /// </summary>
    /// <param name="filePath">The path to the file to delete.</param>
    private static void DeleteIfExists(string filePath)
    {
        FileInfo file = new(filePath);
        if (file.Exists)
        {
            file.Delete();
        }
    }

    /// <summary>
    /// Run the provided process exporting the stdout and stderror back to the user
    /// </summary>
    /// <param name="programName">The name of the program to execute.</param>
    /// <param name="arguments">The parameters to provide to the program.</param>
    /// <returns>The processes' exit code and the outputs from std out and err.</returns>
    private static (int exitCode, string outputText) RunProcess(string programName, string arguments)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(programName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();
        Parallel.Invoke(
            () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                stdOut.AppendLine(process.StandardOutput.ReadLine());
            }
        }, () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                stdErr.AppendLine(process.StandardError.ReadLine());
            }
        });
        process.WaitForExit();
        stdOut.Append(stdErr);
        return (process.ExitCode, stdOut.ToString());
    }
}

