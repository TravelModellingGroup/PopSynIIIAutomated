using System.Diagnostics;

namespace PopSynIIIAutomated;

internal static class MYSQL
{
    /// <summary>
    /// Execute a sql command in mySQL
    /// </summary>
    /// <param name="config"></param>
    /// <param name="sqlCommand"></param>
    /// <returns></returns>
    public static int Execute(Configuration config, string sqlCommand)
    {
        var process = Process.Start($"mysql --user={config.DatabaseUsername} --password={config.DatabasePassword} {config.DatabaseName} -e \"SET sql_mode=''; {sqlCommand}\"");
        process.WaitForExit();
        return process.ExitCode;
    }
}

