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
    /// <param name="additionalHeaders">Optional Additional Headers to use in the meta control file.</param>
    /// <returns>True if the operation succeeds, false otherwise.</returns>
    internal static bool CreateForecastControls(Configuration config, TazControlRecord[] tazControlRecords, string[]? additionalHeaders)
    {
        var fileName = Path.Combine(config.OutputDirectory, "meta_controls.csv");
        using var writer = new StreamWriter(fileName);
        MetaControlRecord.WriteHeader(writer, additionalHeaders);
        foreach (var outputRecord in MetaControlRecord.CreateMetaControlsFrom(tazControlRecords, additionalHeaders))
        {
            outputRecord.Write(writer);
        }
        return true;
    }
}

/// <summary>
/// Contains a record of the meta control.
/// </summary>
/// <param name="Region">The region that is being controlled.</param>
/// <param name="Puma">The puma that this region belongs to.</param>
/// <param name="TotalHouseholds">The total households expected for the region.</param>
/// <param name="TotalPopulation">The total population expected for the region.</param>
/// <param name="AdditionalControls">Optional, the sum of any additional controls.</param>
internal record MetaControlRecord(int Region, int Puma, int TotalHouseholds, int TotalPopulation, float[]? AdditionalControls)
{
    /// <summary>
    /// Creates a meta control record given the TAZ control records.
    /// </summary>
    /// <param name="group">The TAZ control records that belong to this meta control.</param>
    /// <param name="additionalHeaders">Additional headers to use</param>
    /// <returns>A meta control record that sums all of the TAZ control records in the group.</returns>
    internal static MetaControlRecord CreateFrom(IGrouping<int, TazControlRecord> group, string[]? additionalHeaders)
    {
        if (additionalHeaders is not null)
        {
            float[] additionalControls = new float[additionalHeaders.Length];
            foreach (var record in group)
            {
                var a = record.AdditionalControls;
                for (int i = 0; i < a.Length; i++)
                {
                    additionalControls[i] += a[i];
                }
            }
            return new MetaControlRecord(group.Key,
            group.First().Puma,
            group.Sum(record => record.TotalHouseholds),
            group.Sum(record => record.TotalPopulation),
            additionalControls);
        }
        else
        {
            return new MetaControlRecord(group.Key,
                group.First().Puma,
                group.Sum(record => record.TotalHouseholds),
                group.Sum(record => record.TotalPopulation),
                null);
        }
    }

    /// <summary>
    /// Creates meta control records from TAZ control records.
    /// </summary>
    /// <param name="tazControlRecords">The records to build the meta controls from.</param>
    /// <param name="additionalHeaders">Optional, additional headers to use in the meta controls.</param>
    /// <returns>Meta controls ordered by region.</returns>
    internal static MetaControlRecord[] CreateMetaControlsFrom(TazControlRecord[] tazControlRecords, string[]? additionalHeaders)
    {
        return tazControlRecords
            .GroupBy(record => record.Region)
            .Select(group => MetaControlRecord.CreateFrom(group, additionalHeaders))
            .OrderBy(record => record.Region)
            .ToArray();
    }

    /// <summary>
    /// Writes the meta header to the given stream.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    internal static void WriteHeader(StreamWriter writer, string[]? additionalHeaders)
    {
        writer.Write("region,puma,totalhh,totpop");
        if(additionalHeaders is not null)
        {
            for (int i = 0; i < additionalHeaders.Length; i++)
            {
                writer.Write(',');
                writer.Write(additionalHeaders[i]);
            }
        }
        writer.WriteLine();
    }

    /// <summary>
    /// Writes the record to stream.
    /// </summary>
    /// <param name="writer">The stream to write the record to.</param>
    internal void Write(StreamWriter writer)
    {
        WriteThenComma(writer, Region);
        WriteThenComma(writer, Puma);
        WriteThenComma(writer, TotalHouseholds);
        writer.Write(TotalPopulation);
        if (AdditionalControls is not null)
        {
            foreach (var control in AdditionalControls)
            {
                writer.Write(',');
                writer.Write(control);
            }
        }
        writer.WriteLine();
    }
}
