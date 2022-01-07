using System.IO;
using System.Linq;
using static PopSynIIIAutomated.UtilityFunctions;

namespace PopSynIIIAutomated;


/// <summary>
/// Represents a household record outputted from PopSynIII.
/// </summary>
/// <param name="HouseholdId">The id for the household.</param>
/// <param name="TAZ"></param>
/// <param name="DwellingType">The type of dwelling this household lives in.</param>
/// <param name="NumberOfPersons">The number of persons living in this household.</param>
/// <param name="Vehicles">The number of vehicles that this household has.</param>
/// <param name="IncomeClass">The DMG income class the household belongs to.</param>
/// <param name="ExpansionFactor">The expansion of this record to the number of households this represents.</param>
internal record SyntheticHouseholdRecord(int HouseholdId, int TAZ, int DwellingType, int NumberOfPersons, int Vehicles, int IncomeClass, float ExpansionFactor)
{

    /// <summary>
    /// Load in household records from the provided file name.
    /// </summary>
    /// <param name="fileName">The name of the file to load the records from.</param>
    /// <returns>An array of the resulting household records.</returns>
    internal static SyntheticHouseholdRecord[] LoadRecords(string fileName)
    {
        using var reader = new StreamReader(fileName);
        return LoadRecords(reader);
    }

    /// <summary>
    /// Load in household records from the provided stream.
    /// </summary>
    /// <param name="reader">The stream to load in the households from.</param>
    /// <returns>An array of the resulting household records.</returns>
    internal static SyntheticHouseholdRecord[] LoadRecords(StreamReader reader)
    {
         /*
        +-----------------+------------+--------+
        | Field           | Type       | Extra  |
        +-----------------+------------+--------+
        | tempId          | int(11)    |    0   |
        | region          | int(11)    |    1   |
        | puma            | int(11)    |    2   |
        | taz             | int(11)    |    3   |
        | maz             | int(11)    |    4   |
        | weight          | float      |    5   |
        | finalPumsId     | int(11)    |    6   |
        | finalweight     | int(11)    |    7   |
        | DwellingType    | tinyint(4) |    8   |
        | NumberOfPersons | tinyint(4) |    9   |
        | Vehicles        | tinyint(4) |    10  |
        | IncomeClass     | tinyint(4) |    11  |
        +-----------------+------------+--------+  
        */
        return StreamLines(reader)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 12)
            .Select(parts => new SyntheticHouseholdRecord(int.Parse(parts[0]), int.Parse(parts[3]), int.Parse(parts[8]), int.Parse(parts[9]), int.Parse(parts[10]), int.Parse(parts[11]), float.Parse(parts[7])))
            .ToArray();
    }

    /// <summary>
    /// Write the header to the given stream.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    internal static void WriteGTAModelHeader(StreamWriter writer)
    {
        writer.WriteLine("HouseholdId,HouseholdZone,ExpansionFactor,DwellingType,NumberOfPersons,Vehicles,IncomeClass");
    }

    /// <summary>
    /// Output the record to the given stream.
    /// </summary>
    /// <param name="writer">The stream to write to.</param>
    internal void WriteGTAModelRecord(StreamWriter writer)
    {
        WriteThenComma(writer, HouseholdId);
        WriteThenComma(writer, TAZ);
        WriteThenComma(writer, ExpansionFactor);
        WriteThenComma(writer, DwellingType);
        WriteThenComma(writer, NumberOfPersons);
        WriteThenComma(writer, Vehicles);
        writer.WriteLine(IncomeClass);
    }
}
