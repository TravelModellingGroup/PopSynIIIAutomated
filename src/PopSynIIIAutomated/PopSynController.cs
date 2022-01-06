using System;
using System.Diagnostics;
using System.IO;
using static PopSynIIIAutomated.UtilityFunctions;

namespace PopSynIIIAutomated;

/// <summary>
/// Used for controlling the main PopSynIII execution.
/// </summary>
internal static class PopSynController
{
    /// <summary>
    /// Executes PopSynIII.
    /// </summary>
    /// <param name="config">The configuration to run</param>
    /// <returns>True if the process completes.</returns>
    internal static bool Run(Configuration config)
    {
        // Setup database (replaces runPopSynIII.bat)
        InitializeDatabase(config);
        /// Execute PopSynIII
        ExecutePopSynIIIRun(config);
        return true;
    }

    private static bool InitializeDatabase(Configuration config)
    {
        var pumsHH_File = AddQuotes(Path.Combine(config.InputDirectory, "BaseYearInput/hhtable.csv"));
        var pumsPersonsFile = AddQuotes(Path.Combine(config.InputDirectory, "BaseYearInput/perstable.csv"));
        var mazControls = AddQuotes(Path.Combine(config.OutputDirectory, "mazControls.csv"));
        var tazControls = AddQuotes(Path.Combine(config.OutputDirectory, "tazControls.csv"));
        var metaControls = AddQuotes(Path.Combine(config.OutputDirectory, "meta_controls.csv"));

        return
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\PUMFTableCreation.sql" > "%MY_PATH%\outputs\serverLog"
        MYSQL.ExecuteScript(config, $"\"{config.InputDirectory}\"/scripts/PUMFTableCreation.sql")
        // Upload Data From CSV Files
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %pumsHH_File% INTO TABLE pumf_hh FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %pumsPersons_File% INTO TABLE pumf_person FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
        && MYSQL.ExecuteCommand(config, $"LOAD DATA INFILE {pumsHH_File} INTO TABLE pumf_hh FIELDS TERMINATED BY ',' LINES TERMINATED BY '\\n' IGNORE 1 LINES;")
        && MYSQL.ExecuteCommand(config, $"LOAD DATA INFILE {pumsPersonsFile} INTO TABLE pumf_person FIELDS TERMINATED BY ',' LINES TERMINATED BY '\\n' IGNORE 1 LINES;")
        // 
        // ECHO Processing PUMS tables...
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\PUMFTableProcessing.sql" > "%MY_PATH%\outputs\serverLog"
        && MYSQL.ExecuteScript(config, $"\"{config.InputDirectory}\"/scripts/PUMFTableProcessing.sql")
        // 
        // ECHO Creating Control Tables..CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\ControlsTableCreation.sql" > "%MY_PATH%\outputs\serverLog"
        // 
        // 
        // ECHO Uploading Control Tables...
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %mazData_File% INTO TABLE control_totals_maz FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %tazData_File% INTO TABLE control_totals_taz FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %metaData_File% INTO TABLE control_totals_meta FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
        && MYSQL.ExecuteCommand(config, $"LOAD DATA INFILE {mazControls} INTO TABLE control_totals_maz FIELDS TERMINATED BY ',' LINES TERMINATED BY '\\n' IGNORE 1 LINES;")
        && MYSQL.ExecuteCommand(config, $"LOAD DATA INFILE {tazControls} INTO TABLE control_totals_taz FIELDS TERMINATED BY ',' LINES TERMINATED BY '\\n' IGNORE 1 LINES;")
        && MYSQL.ExecuteCommand(config, $"LOAD DATA INFILE {metaControls} INTO TABLE control_totals_meta FIELDS TERMINATED BY ',' LINES TERMINATED BY '\\n' IGNORE 1 LINES;")

        // 
        // ECHO Processing Control Tables...
        // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\ControlsTableProcessing.sql" > "%MY_PATH%\outputs\serverLog"
        &&MYSQL.ExecuteScript(config, $"\"{config.InputDirectory}\"/scripts/PUMFTableProcessing.sql")
        ;
    }

    private static bool ExecutePopSynIIIRun(Configuration config)
    {
        // %JAVA_64_PATH%\bin\java -showversion -server -Xms8000m -Xmx15000m -cp "%CLASSPATH%"
        // -Djppf.config=jppf-clientLocal.properties -Djava.library.path=%LIBPATH% popGenerator.PopGenerator BaseYearData/settings.xml
        var locationOfJava = Path.Combine(config.JavaDirectory, "bin/java");
        var classPath = "runtime/config; runtime/*; runtime/lib/*; runtime/lib/JPFF-3.2.2/JPPF-3.2.2-admin-ui/lib/*;";
        var libPath = Path.Combine(Environment.CurrentDirectory, "runtime", "lib");
        var settingsFile = Path.Combine(config.InputDirectory, "BaseYearData", "settings.xml");
        var arguments = $"-showversion -server -Xms8000m -Xmx15000m -cp \"{classPath}\" -Djppf.config=jppf-clientLocal.properties " +
            $"-Djava.library.path=\"{libPath}\" popGenerator.PopGenerator \"{settingsFile}\"";

        var process = new Process();
        process.StartInfo.FileName = locationOfJava;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.EnvironmentVariables["PATH"] += ";" + libPath; 
        if(!process.Start())
        {
            return false;
        }
        process.WaitForExit();
        return process.ExitCode == 0;
    }
}
