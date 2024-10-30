using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static PopSynIIIAutomated.UtilityFunctions;

namespace PopSynIIIAutomated;

/// <summary>
/// This class provides access to generating the updated control files.
/// </summary>
internal static class TazControlFile
{
    /// <summary>
    /// Create the updated control files for TAZ and MAZ
    /// </summary>
    /// <param name="configuration">The configuration for the run.</param>
    /// <param name="zoneSystem">The zone system we are working with.</param>
    /// <param name="forecastPopulation">The future year population by TAZ.</param>
    /// <returns>True if the operation succeeds, false otherwise.</returns>
    public static bool CreateForecastControls(Configuration configuration, ZoneSystem zoneSystem,
        Dictionary<int, float> forecastPopulation, out string[]? additionalHeaders)
    {
        const float minimumBaseYearPopulation = 50;
        var lines = File.ReadAllLines(Path.Combine(configuration.InputDirectory, "BaseYearData/taz_controls.csv"));
        var zoneRecords = TazControlRecord.LoadRecordsFromLines(lines, out additionalHeaders);
        var pumaRecords = ComputePUMAAverages(zoneRecords);
        var outputFilePath = CreateDirectories(Path.Combine(configuration.OutputDirectory, "taz_controls.csv"));
        using (var writer = new StreamWriter(outputFilePath))
        {
            // region,puma,taz,totalhh,totpop,income_class_1,income_class_2,income_class_3,income_class_4,income_class_5,income_class_6,male,female
            TazControlRecord.WriteHeader(writer, lines[0]);
            foreach (var record in zoneRecords)
            {
                var zone = zoneSystem.GetZone(zoneSystem.ZoneNumberToZoneIndex(record.TAZ));
                float targetPopulation = forecastPopulation[zone.TAZ];
                // Two cases:
                // If there was already population in the base year just scale up the zone's inputs                    
                // If there was not, compute the average
                if (zone.BaseYearPopulation > minimumBaseYearPopulation)
                {
                    // Scale the individual record
                    record.WriteScaled(writer, targetPopulation / zone.BaseYearPopulation);
                }
                else
                {
                    // Use the PUMA's average
                    var pumaRecord = pumaRecords[record.Puma];
                    pumaRecord.WriteScaled(writer, targetPopulation / pumaRecord.TotalPopulation,
                        record.TAZ, record.Region);
                }
            }
        }
        // Duplicate the TAZ controls to MAZ controls to just make this work
        File.Copy(outputFilePath, Path.Combine(configuration.OutputDirectory, "maz_controls.csv"), true);
        return true;
    }

    /// <summary>
    /// Computes the average values for each PUMA
    /// </summary>
    /// <param name="zoneRecords">The records for the base year for each TAZ.</param>
    /// <returns>A dictionary keyed by PUMA number of an average record for the PUMA</returns>
    private static Dictionary<int, TazControlRecord> ComputePUMAAverages(TazControlRecord[] zoneRecords)
    {
        return zoneRecords
            .GroupBy(record => record.Puma)
            .Select(group => TazControlRecord.ComputeGroupAverages(group))
            .ToDictionary(avgRecord => avgRecord.Puma, avgRecord => avgRecord);
    }
}

/// <summary>
/// Represents a control record for a TAZ
/// </summary>
/// <param name="Region">The region that the TAZ is in.</param>
/// <param name="Puma">The PUMA that the TAZ is in.</param>
/// <param name="TAZ">The TAZ number.</param>
/// <param name="TotalHouseholds">The total number of households in the TAZ</param>
/// <param name="TotalPopulation">The total population in the TAZ.</param>
/// <param name="AdditionalControls">Additional controls for the TAZ</param>
internal record TazControlRecord(int Region, int Puma, int TAZ, int TotalHouseholds, int TotalPopulation, int[] AdditionalControls)
{
    /// <summary>
    /// Load Taz Control Records from the given lines of CSV text.
    /// </summary>
    /// <param name="lines">The lines to process.</param>
    /// <returns>An array of TAZ control records for each non-header line.</returns>
    internal static TazControlRecord[] LoadRecordsFromLines(string[] lines, out string[]? additionalHeaders)
    {
        var ret = new TazControlRecord[lines.Length - 1];
        if(lines.Length == 0)
        {
            additionalHeaders = null;
            return ret;
        }
        // Read the additional headers
        static string[]? GetAdditionalHeaders(string headerLine)
        {
            var split = headerLine.Split(',');
            if(split.Length <= 6)
            {
                return null;
            }
            return split.Skip(6).ToArray();
        }
        additionalHeaders = GetAdditionalHeaders(lines[0]);
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            ret[i - 1] = new TazControlRecord(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[4]),
                int.Parse(parts[5]),
                // Read the rest
                parts.Skip(6).Select(x => int.Parse(x)).ToArray()
            );
        }
        return ret;
    }

    /// <summary>
    /// Computes a TAZ control that is the average of the given group.
    /// </summary>
    /// <param name="group">The group of TAZ records to process, indexed by PUMA.</param>
    /// <returns>An averaged TAZ control record for the given group.</returns>
    internal static TazControlRecord ComputeGroupAverages(IGrouping<int, TazControlRecord> group)
    {
        float scaleFactor = 1.0f / group.Count();
        var additionalControls = new int[group.First().AdditionalControls.Length];
        for (int i = 0; i < additionalControls.Length; i++)
        {
            additionalControls[i] = Scale(group.Sum(record => record.AdditionalControls[i]), scaleFactor);
        }
        return new TazControlRecord(-1, group.Key, -1,
                Scale(group.Sum(record => record.TotalHouseholds), scaleFactor),
                Scale(group.Sum(record => record.TotalPopulation), scaleFactor),
                additionalControls);
    }

    /// <summary>
    /// Write the TAZ control to the given stream scaled by the scaling factor.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="scaleFactor">The factor to scale the records by.</param>
    internal void WriteScaled(StreamWriter writer, float scaleFactor)
    {
        WriteScaled(writer, scaleFactor, TAZ, Region);
    }

    /// <summary>
    /// Writes out the header for the given TAZ control header line.
    /// </summary>
    /// <param name="writer">The stream to save the data to.</param>
    /// <param name="headerLine">The base year header line.</param>
    internal static void WriteHeader(StreamWriter writer, string headerLine)
    {
        var headerParts = headerLine.Split(',');
        writer.Write("region,puma,taz,maz,totalhh,totpop");
        // Write out all of the additional properties
        for(int i = 6;i < headerParts.Length;i++)
        {
            writer.Write(',');
            writer.Write(headerParts[i]);
        }
        writer.WriteLine();
    }

    /// <summary>
    /// Write the TAZ control to the given stream scaled by the scaling factor.
    /// The TAZ and Region code will be replaced by the given values.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="scaleFactor">The factor to scale the records by.</param>
    /// <param name="taz">The TAZ number to inject for this record.</param>
    /// <param name="region">The region number to inject for this record.</param>
    internal void WriteScaled(StreamWriter writer, float scaleFactor, int taz, int region)
    {
        var population = Scale(TotalPopulation, scaleFactor);
        var households = Scale(TotalHouseholds, scaleFactor);
        WriteThenComma(writer, region);
        WriteThenComma(writer, Puma);
        WriteThenComma(writer, taz);
        WriteThenComma(writer, taz);
        if(population > 0)
        {
            // If there is a person living in the zone we need to have a house for them.
            WriteThenComma(writer, Math.Max(households, 1));
            WriteThenComma(writer, population);
            if (AdditionalControls.Length > 0)
            {
                for (int i = 0; i < AdditionalControls.Length - 1; i++)
                {
                    WriteThenComma(writer, AdditionalControls[i], scaleFactor);
                }
                writer.WriteLine(Scale(AdditionalControls[^1], scaleFactor));
            }
        }
        else
        {
            writer.Write("0,0,");
            if (AdditionalControls.Length > 0)
            {
                for (int i = 0; i < AdditionalControls.Length - 1; i++)
                {
                    WriteThenComma(writer, 0);
                }
                writer.WriteLine(0);
            }
        }
    }
}
