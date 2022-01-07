using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PopSynIIIAutomated;

internal static class Postprocessor
{
    /// <summary>
    /// Run processes that should complete after the main PopSynIII run completes.
    /// </summary>
    /// <param name="config">The configuration to run</param>
    /// <returns>True if the process completes.</returns>
    internal static bool Run(Configuration config)
    {
        // Take the PopSynIII household and person records and transform them into
        // inputs for GTAModel.
        Directory.CreateDirectory(Path.Combine(config.OutputDirectory, "HouseholdData"));
        Parallel.Invoke(
            // Process households
            () =>
            {
                ProcessHouseholds(config);
            },
            // Process Persons
            () =>
            {
                ProcessPersons(config);
            }
        );
        return true;
    }

    /// <summary>
    /// Write the household records as GTAModel records.
    /// </summary>
    /// <param name="config">The run's configuration.</param>
    private static void ProcessHouseholds(Configuration config)
    {
        var households = SyntheticHouseholdRecord.LoadRecords(Path.Combine(config.OutputDirectory, "Households.csv"));
        using var writer = new StreamWriter(Path.Combine(config.OutputDirectory, "HouseholdData", "Households.csv"));
        SyntheticHouseholdRecord.WriteGTAModelHeader(writer);
        foreach (var household in households)
        {
            household.WriteGTAModelRecord(writer);
        }
    }

    /// <summary>
    /// Write the person records and zonal residence.
    /// </summary>
    /// <param name="config">The run's configuration</param>
    private static void ProcessPersons(Configuration config)
    {
        var occEmpCounts = CreateOccEmpCounts();
        var persons = SyntheticPersonRecord.LoadRecords(Path.Combine(config.OutputDirectory, "Persons.csv"));
        using var writer = new StreamWriter(Path.Combine(config.OutputDirectory, "HouseholdData", "Persons.csv"));
        SyntheticPersonRecord.WriteGTAModelHeader(writer);
        foreach (var person in persons)
        {
            ProcessZonalResidence(occEmpCounts, person);
            person.WriteGTAModelRecord(writer);
        }
        WriteZonalResidence(config, occEmpCounts);
    }

    /// <summary>
    /// Accumulate the person's employment into the occupation employment counts.
    /// </summary>
    /// <param name="occEmpCounts">The OccupationCount array.</param>
    /// <param name="person">The person to accumulate.</param>
    private static void ProcessZonalResidence(Dictionary<int, float>[] occEmpCounts, SyntheticPersonRecord person)
    {
        var index = GetOccEmpIndex(person);
        if(index < 0)
        {
            return;
        }
        var homeZone = person.TAZ;
        occEmpCounts[index].TryGetValue(homeZone, out float employedPersons);
        occEmpCounts[index][homeZone] = employedPersons + person.ExpansionFactor;
    }

    /// <summary>
    /// Write out the completed occupation employment counts.
    /// </summary>
    /// <param name="config">The run's configuration</param>
    private static void WriteZonalResidence(Configuration config, Dictionary<int, float>[] occEmpCounts)
    {
        string[] occEmpNames = new[] { "GF", "SF", "MF", "PF", "GP", "SP", "MP", "PP" };
        Directory.CreateDirectory(Path.Combine(config.OutputDirectory, "ZonalResidence"));
        for (int i = 0; i < occEmpCounts.Length; i++)
        {
            using var writer = new StreamWriter(Path.Combine(config.OutputDirectory, "ZonalResidence", occEmpNames[i] + ".csv"));
            writer.WriteLine("HomeZone,EmployedPersons");
            foreach(var entry in occEmpCounts[i]
                .OrderBy(entry => entry.Key))
            {
                writer.Write(entry.Key);
                writer.Write(',');
                writer.WriteLine(entry.Value);
            }
        }
    }

    /// <summary>
    /// Initialize the occupation employment counts
    /// </summary>
    /// <returns>An array of dictionaries, one per occemp class.</returns>
    private static Dictionary<int, float>[] CreateOccEmpCounts()
    {
        Dictionary<int, float>[] occEmpCounts = new Dictionary<int, float>[8];
        for (int i = 0; i < occEmpCounts.Length; i++)
        {
            occEmpCounts[i] = new Dictionary<int, float>();
        }
        return occEmpCounts;
    }

    /// <summary>
    /// Gets the occupation employment dictionary array index to write to.
    /// </summary>
    /// <param name="person">The person to get the index for.</param>
    /// <returns>The index to use, -1 if they are not employed.</returns>
    private static int GetOccEmpIndex(SyntheticPersonRecord person)
    {
        // "GF", "SF", "MF", "PF", "GP", "SP", "MP", "PP"
        //  0     1     2     3     4    5     6     7

        // You can refer to the code using the convert methods in
        // synthetic persons.
        return (person.Occupation, person.EmploymentStatus) switch
        {
            // Full-Time
            (1, 2) => 0,
            (2, 2) => 1,
            (3, 2) => 2,
            (4, 2) => 3,

            // Part-Time
            (1, 3) => 4,
            (2, 3) => 5,
            (3, 3) => 6,
            (4, 3) => 7,

            // We don't record anything else
            _ => -1
        };
    }
}
