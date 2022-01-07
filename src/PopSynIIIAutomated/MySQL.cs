using System;
using System.Diagnostics;

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
        var process = Process.Start("mysql", $"--user={config.DatabaseUsername} --password={config.DatabasePassword} {config.DatabaseName} -e \"SET sql_mode=''; {sqlCommand}\"");
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    /// <summary>
    /// Execute a sql script in MySQL
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <param name="scriptPath">The full path to the script file.</param>
    /// <returns>If the operation completed successfully.</returns>
    public static bool ExecuteScript(Configuration config, string scriptPath)
    {
        var process = Process.Start("mysql", $"--user={config.DatabaseUsername} --password={config.DatabasePassword} {config.DatabaseName} -e \"script {scriptPath}\"");
        process.WaitForExit();
        return process.ExitCode == 0;
    }
}

