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
        Dictionary<int, float> forecastPopulation)
    {
        var lines = File.ReadAllLines(Path.Combine(configuration.InputDirectory, "BaseYearData/taz_controls.csv"));
        var zoneRecords = TazControlRecord.LoadRecordsFromLines(lines);
        var pumaRecords = ComputePUMAAverages(zoneRecords);
        var outputFilePath = CreateDirectories(Path.Combine(configuration.OutputDirectory, "taz_controls.csv"));
        using (var writer = new StreamWriter(outputFilePath))
        {
            // region,puma,taz,totalhh,totpop,income_class_1,income_class_2,income_class_3,income_class_4,income_class_5,income_class_6,male,female
            // write out the header
            writer.WriteLine(lines[0]);
            foreach (var record in zoneRecords)
            {
                var zone = zoneSystem.GetZone(zoneSystem.ZoneNumberToZoneIndex(record.TAZ));
                float targetPopulation = forecastPopulation[zone.TAZ];
                // Two cases:
                // If there was already population in the base year just scale up the zone's inputs                    
                // If there was not, compute the average
                if (zone.BaseYearPopulation > 0.0f)
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
/// <param name="Income_classes">The number of households that belong to each income class.</param>
/// <param name="Male">The number of males living in the TAZ.</param>
/// <param name="Female">The number of females living in the TAZ.</param>
internal record TazControlRecord(int Region, int Puma, int TAZ, int TotalHouseholds, int TotalPopulation, int[] Income_classes, int Male, int Female)
{
    /// <summary>
    /// Load Taz Control Records from the given lines of CSV text.
    /// </summary>
    /// <param name="lines">The lines to process.</param>
    /// <returns>An array of TAZ control records for each non-header line.</returns>
    internal static TazControlRecord[] LoadRecordsFromLines(string[] lines)
    {
        var ret = new TazControlRecord[lines.Length - 1];
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            ret[i - 1] = new TazControlRecord(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[3]),
                int.Parse(parts[4]),
                new int[]
                {
                    int.Parse(parts[6]),
                    int.Parse(parts[7]),
                    int.Parse(parts[8]),
                    int.Parse(parts[9]),
                    int.Parse(parts[10]),
                    int.Parse(parts[5]),
                },
                int.Parse(parts[11]),
                int.Parse(parts[12])
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
        return new TazControlRecord(-1, group.Key, -1,
                Scale(group.Sum(record => record.TotalHouseholds), scaleFactor),
                Scale(group.Sum(record => record.TotalPopulation), scaleFactor),
                new int[]
                {
                    Scale(group.Sum(record=> record.Income_classes[0]), scaleFactor),
                    Scale(group.Sum(record=> record.Income_classes[1]), scaleFactor),
                    Scale(group.Sum(record=> record.Income_classes[2]), scaleFactor),
                    Scale(group.Sum(record=> record.Income_classes[3]), scaleFactor),
                    Scale(group.Sum(record=> record.Income_classes[4]), scaleFactor),
                    Scale(group.Sum(record=> record.Income_classes[5]), scaleFactor)
                },
                Scale(group.Sum(record => record.Male), scaleFactor),
                Scale(group.Sum(record => record.Female), scaleFactor)
            );
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
    /// Write the TAZ control to the given stream scaled by the scaling factor.
    /// The TAZ and Region code will be replaced by the given values.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    /// <param name="scaleFactor">The factor to scale the records by.</param>
    /// <param name="taz">The TAZ number to inject for this record.</param>
    /// <param name="region">The region number to inject for this record.</param>
    internal void WriteScaled(StreamWriter writer, float scaleFactor, int taz, int region)
    {
        WriteThenComma(writer, region);
        WriteThenComma(writer, Puma);
        WriteThenComma(writer, taz);
        WriteThenComma(writer, TotalHouseholds, scaleFactor);
        WriteThenComma(writer, TotalPopulation, scaleFactor);
        for (int i = 0; i < Income_classes.Length; i++)
        {
            WriteThenComma(writer, Income_classes[i], scaleFactor);
        }
        WriteThenComma(writer, Male, scaleFactor);
        writer.WriteLine(Scale(Female, scaleFactor));
    }
}
