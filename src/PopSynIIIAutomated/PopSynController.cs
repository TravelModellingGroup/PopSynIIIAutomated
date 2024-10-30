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
        return
            InitializeDatabase(config)
            && ExecutePopSynIIIRun(config)
            && ExportResults(config);
    }

    /// <summary>
    /// Initialize the Database with our population and control totals.
    /// </summary>
    /// <param name="config">The configuration to run.</param>
    /// <returns>True if the operation succeeds.</returns>
    private static bool InitializeDatabase(Configuration config)
    {
        var pumsHH_File = Path.Combine(config.InputDirectory, "BaseYearData/hhtable.csv");
        var pumsPersonsFile = Path.Combine(config.InputDirectory, "BaseYearData/perstable.csv");
        var mazControls = Path.Combine(config.OutputDirectory, "maz_Controls.csv");
        var tazControls = Path.Combine(config.OutputDirectory, "taz_Controls.csv");
        var metaControls = Path.Combine(config.OutputDirectory, "meta_controls.csv");
        Runtime.WriteToUser($"Initializing Database");
        return
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\PUMFTableCreation.sql" > "%MY_PATH%\outputs\serverLog"
            MYSQL.ExecuteScript(config, Path.Combine(config.InputDirectory, "scripts/PUMFTableCreation.sql"), "Creating PUMF tables.")
            // Upload Data From CSV Files
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %pumsHH_File% INTO TABLE pumf_hh FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %pumsPersons_File% INTO TABLE pumf_person FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
            && MYSQL.LoadTableFromFile(config, "pumf_hh", pumsHH_File)
            && MYSQL.LoadTableFromFile(config, "pumf_person", pumsPersonsFile)
            // ECHO Processing PUMS tables...
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\PUMFTableProcessing.sql" > "%MY_PATH%\outputs\serverLog"
            && MYSQL.ExecuteScript(config, Path.Combine(config.InputDirectory, "scripts/PUMFTableProcessing.sql"), "Processing PUMF Tables")
            // ECHO Creating Control Tables..
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "source %MY_PATH%\scripts\ControlsTableCreation.sql" > "%MY_PATH%\outputs\serverLog"
            && MYSQL.ExecuteScript(config, Path.Combine(config.InputDirectory, "scripts/ControlsTableCreation.sql"), "Creating Control Tables")
            // ECHO Uploading Control Tables...
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %mazData_File% INTO TABLE control_totals_maz FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %tazData_File% INTO TABLE control_totals_taz FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
            // CALL % MYSQL_EXE % --host =% SQLSERVER % --user =% DB_USER % --password =% DB_PWD % % DATABASE % -e "SET sql_mode=''; LOAD DATA INFILE %metaData_File% INTO TABLE control_totals_meta FIELDS TERMINATED BY ',' LINES TERMINATED BY '\n' IGNORE 1 LINES;" > "%MY_PATH%\outputs\serverLog"
            && MYSQL.LoadTableFromFile(config, "control_totals_maz", mazControls)
            && MYSQL.LoadTableFromFile(config, "control_totals_taz", tazControls)
            && MYSQL.LoadTableFromFile(config, "control_totals_meta", metaControls)
        ;
    }

    /// <summary>
    /// Execute the PopSynIII java program.
    /// </summary>
    /// <param name="config">The configuration to run.</param>
    /// <returns>True if the operation succeeds, false otherwise.</returns>
    private static bool ExecutePopSynIIIRun(Configuration config)
    {
        // %JAVA_64_PATH%\bin\java -showversion -server -Xms8000m -Xmx15000m -cp "%CLASSPATH%"
        // -Djppf.config=jppf-clientLocal.properties -Djava.library.path=%LIBPATH% popGenerator.PopGenerator BaseYearData/settings.xml
        Runtime.WriteToUser("Starting PopSynIII");
        try
        {
            var locationOfJava = Path.Combine(config.JavaDirectory, "bin/java");
            var classPath = "runtime/config;runtime/*;runtime/lib/*;runtime/lib/JPFF-3.2.2/JPPF-3.2.2-admin-ui/lib/*;";
            var libPath = Path.Combine(Environment.CurrentDirectory, "runtime", "lib");
            var settingsFile = Path.Combine(config.InputDirectory, "BaseYearData", "settings.xml");
            var arguments = $"-showversion -server -Xms8000m -Xmx15000m -cp \"{classPath}\" -Djppf.config=jppf-clientLocal.properties " +
                $"-Djava.library.path=\"{libPath}\" popGenerator.PopGenerator \"{settingsFile}\"";
            Runtime.WriteToUser(locationOfJava);
            Runtime.WriteToUser(arguments);
            return UtilityFunctions.RunProcess(locationOfJava, config.InputDirectory, arguments, libPath) == 0;
        }
        catch (Exception ex)
        {
            Runtime.WriteToUser(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Export the Households and Persons files from the database
    /// </summary>
    /// <param name="config">The configuration to run.</param>
    /// <returns>True if the operation succeeds.</returns>
    private static bool ExportResults(Configuration config)
    {
        // We don't write out to the HouseholdData folder during this step
        var householdFile = Path.Combine(config.OutputDirectory, "Households.csv");
        var personsFile = Path.Combine(config.OutputDirectory, "Persons.csv");
        Runtime.WriteToUser("Exporting Household and Person records.");
        return MYSQL.ExportTable(config, "Households", householdFile)
            && MYSQL.ExportTable(config, "Persons", personsFile);
    }
}
