using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PopSynIIIAutomated;

internal static class Preprocessor
{
    /// <summary>
    /// Setup the input files for PopSynIII
    /// </summary>
    /// <param name="config">The configuration to run.</param>
    /// <returns>True if the process completes.</returns>
    internal static bool Run(Configuration config)
    {
        // Read in Zone System
        var zones = new ZoneSystem(config);
        // Read in Forecast Population vector
        var forecast = LoadForecast(config);
        // Write updated TAZ/MAZ Controls
        return TazControlFile.CreateForecastControls(config, zones, forecast, out var additionalHeaders)
            // Write updated Meta Controls
            && MetaControlFile.CreateForecastControls(config, LoadForecastedTazControlRecords(config), additionalHeaders);
    }

    /// <summary>
    /// Load in the forecast population from the scenario directory.
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <returns>A dictionary mapping TAZ to forecast population.</returns>
    internal static Dictionary<int, float> LoadForecast(Configuration config)
    {
        var fileName = Path.Combine(config.ScenarioDirectory, "Population.csv");
        return File.ReadAllLines(fileName)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where (parts => parts.Length > 0)
            .ToDictionary(parts => int.Parse(parts[0]), parts => float.Parse(parts[1]));
    }

    internal static TazControlRecord[] LoadForecastedTazControlRecords(Configuration config)
    {
        var fileName = Path.Combine(config.OutputDirectory, "taz_controls.csv");
        // Ignore the additional headers
        return TazControlRecord.LoadRecordsFromLines(File.ReadAllLines(fileName), out var _);
    }
}

