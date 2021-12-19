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
    public static bool CreateForecastTazControls(Configuration configuration, ZoneSystem zoneSystem,
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

    internal record TazControlRecord(int Region, int Puma, int TAZ, int TotalHouseholds, int TotalPopulation, int[] Income_classes, int Male, int Female)
    {
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
                            int.Parse(parts[5]),
                            int.Parse(parts[6]),
                            int.Parse(parts[7]),
                            int.Parse(parts[8]),
                            int.Parse(parts[9]),
                            int.Parse(parts[10]),
                    },
                    int.Parse(parts[11]),
                    int.Parse(parts[12])
                );
            }
            return ret;
        }

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

        internal void WriteScaled(StreamWriter writer, float scaleFactor)
        {
            WriteScaled(writer, scaleFactor, TAZ, Region);
        }

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
}

