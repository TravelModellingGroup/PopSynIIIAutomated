using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static PopSynIIIAutomated.UtilityFunctions;

namespace PopSynIIIAutomated;

/// <summary>
/// This class is designed to generate an updated Meta Control file.
/// </summary>
internal static class MetaControlFile
{
    /// <summary>
    /// Create a new meta control file given the run's configuration and the TAZ controls.
    /// </summary>
    /// <param name="config">The run's configuration.</param>
    /// <param name="tazControlRecords">The TAZ control records for the run.</param>
    /// <returns>True if the operation succeeds, false otherwise.</returns>
    internal static bool CreateForecastControls(Configuration config, TazControlRecord[] tazControlRecords)
    {
        var fileName = Path.Combine(config.OutputDirectory, "meta_controls.csv");
        using var writer = new StreamWriter(fileName);
        MetaControlRecord.WriteHeader(writer);
        foreach (var outputRecord in MetaControlRecord.CreateMetaControlsFrom(tazControlRecords))
        {
            outputRecord.Write(writer);
        }
        return true;
    }
}

/// <summary>
/// Contains a record of the meta control.
/// </summary>
/// <param name="Puma">The region that is being controlled.</param>
/// <param name="TotalHouseholds">The total households expected for the region.</param>
/// <param name="TotalPopulation">The total population expected for the region.</param>
internal record MetaControlRecord(int Puma, int TotalHouseholds, int TotalPopulation)
{
    /// <summary>
    /// Creates a meta control record given the TAZ control records.
    /// </summary>
    /// <param name="group">The TAZ control records that belong to this meta control.</param>
    /// <returns>A meta control record that sums all of the TAZ control records in the group.</returns>
    internal static MetaControlRecord CreateFrom(IGrouping<int, TazControlRecord> group)
    {
        return new MetaControlRecord(group.Key,
            group.Sum(record => record.TotalHouseholds),
            group.Sum(record => record.TotalPopulation));
    }

    /// <summary>
    /// Creates meta control records from TAZ control records.
    /// </summary>
    /// <param name="tazControlRecords">The records to build the meta controls from.</param>
    /// <returns>Meta controls ordered by PUMA.</returns>
    internal static MetaControlRecord[] CreateMetaControlsFrom(TazControlRecord[] tazControlRecords)
    {
        return tazControlRecords
            .GroupBy(record => record.Puma)
            .Select(group => MetaControlRecord.CreateFrom(group))
            .OrderBy(record => record.Puma)
            .ToArray();
    }

    /// <summary>
    /// Writes the meta header to the given stream.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    internal static void WriteHeader(StreamWriter writer)
    {
        writer.WriteLine("PUMA,totalhh,totpop");
    }

    /// <summary>
    /// Writes the record to stream.
    /// </summary>
    /// <param name="writer">The stream to write the record to.</param>
    internal void Write(StreamWriter writer)
    {
        WriteThenComma(writer, Puma);
        WriteThenComma(writer, TotalHouseholds);
        writer.WriteLine(TotalPopulation);
    }
}
