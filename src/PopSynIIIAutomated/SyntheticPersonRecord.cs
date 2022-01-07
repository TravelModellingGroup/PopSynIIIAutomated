using System;
using System.IO;
using System.Linq;
using static PopSynIIIAutomated.UtilityFunctions;

namespace PopSynIIIAutomated;

/// <summary>
/// Represents a record output from PopSynIII
/// </summary>
/// <param name="TAZ"></param>
/// <param name="HouseholdId">The household id that this </param>
/// <param name="PersonNumber">The unique number within the household for this individual.</param>
/// <param name="Age">The age of the person.</param>
/// <param name="Sex">The sex of the person. 1 is female, 2 is male.</param>
/// <param name="EmploymentStatus">The employment status of the person.  Refer to the convert method for decoding.</param>
/// <param name="Occupation">The occupation of the person.  Refer to the convert method for decoding.</param>
/// <param name="StudentStatus">The student status of the person.  Refer to the convert method for decoding.</param>
/// <param name="EmploymentZone">The zone the record is employed in.</param>
/// <param name="ExpansionFactor"></param>
internal record SyntheticPersonRecord(int TAZ, int HouseholdId, int PersonNumber, int Age,
    int Sex, int EmploymentStatus, int Occupation, int StudentStatus, int EmploymentZone,
    int SchoolZone, float ExpansionFactor)
{
    /// <summary>
    /// Load in person records from the provided file name.
    /// </summary>
    /// <param name="fileName">The name of the file to load the records from.</param>
    /// <returns>An array of the resulting household records.</returns>
    internal static SyntheticPersonRecord[] LoadRecords(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return LoadRecords(reader);
    }

    /// <summary>
    /// Load in person records from the provided stream.
    /// </summary>
    /// <param name="reader">The stream to load the person records from.</param>
    /// <returns>An array of the resulting person records.</returns>
    internal static SyntheticPersonRecord[] LoadRecords(StreamReader reader)
    {
        /*
        +------------------+---------+-------+
        | Field            | Type    | Extra |
        +------------------+---------+-------+
        | tempId           | int(11) |  00   |
        | region           | int(11) |  01   |
        | puma             | int(11) |  02   |
        | taz              | int(11) |  03   |
        | maz              | int(11) |  04   |
        | weight           | float   |  05   |
        | finalPumsId      | int(11) |  06   |
        | finalweight      | int(11) |  07   |
        | PersonNumber     | int(11) |  08   |
        | Age              | int(11) |  09   |
        | Sex              | int(11) |  10   |
        | License          | int(11) |  11   |
        | TransitPass      | int(11) |  12   |
        | EmploymentStatus | int(11) |  13   |
        | Occupation       | int(11) |  14   |
        | FreeParking      | int(11) |  15   |
        | StudentStatus    | int(11) |  16   |
        | EmploymentZone   | int(11) |  17   |
        | SchoolZone       | int(11) | 18    |
        +------------------+---------+-------+
        */
        return UtilityFunctions.StreamLines(reader)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 18)
            .Select(parts => new SyntheticPersonRecord(
                int.Parse(parts[3]),
                int.Parse(parts[0]),
                int.Parse(parts[8]),
                int.Parse(parts[9]),
                int.Parse(parts[10]),
                int.Parse(parts[13]),
                int.Parse(parts[14]),
                int.Parse(parts[16]),
                int.Parse(parts[17]),
                int.Parse(parts[18]),
                float.Parse(parts[7])
                ))
            .ToArray();
    }

    /// <summary>
    /// Converts the occupation value to GTAModel code
    /// </summary>
    /// <returns>The GTAModel code for the occupation class</returns>
    internal char ConvertOccupation()
    {
        return Occupation switch
        {
            1 => 'G',
            2 => 'S',
            3 => 'M',
            4 => 'P',
            _ => 'O'
        };
    }

    /// <summary>
    /// Converts the employment status value to GTAModel code
    /// </summary>
    /// <returns>The GTAModel code for the employment status</returns>
    internal char ConvertEmploymentStatus()
    {
        return EmploymentStatus switch
        {
            2 => 'F',
            3 => 'P',
            4 => 'J',
            5 => 'H',
            _ => 'O',
        };
    }

    /// <summary>
    /// Converts the student status value to GTAModel code
    /// </summary>
    /// <returns>The GTAModel code for the student status</returns>
    internal char ConvertStudentStatus()
    {
        return StudentStatus switch
        {
            1 => 'F',
            2 => 'P',
            _ => 'O'
        };
    }

    /// <summary>
    /// Converts the sex value to the GTAModel code
    /// </summary>
    /// <returns>The GTAModel code of the sex value</returns>
    internal char ConvertSex()
    {
        return Sex switch
        {
            1 => 'F',
            _ => 'M'
        };
    }

    /// <summary>
    /// Writes the CSV header to the given stream.
    /// </summary>
    /// <param name="writer">The stream to write the header to</param>
    internal static void WriteGTAModelHeader(StreamWriter writer)
    {
        writer.WriteLine("HouseholdId,PersonNumber,Age,Sex,License,TransitPass,EmploymentStatus," +
            "Occupation,FreeParking,StudentStatus,EmploymentZone,SchoolZone,ExpansionFactor");
    }

    /// <summary>
    /// Writes the CSV record to the given stream.
    /// </summary>
    /// <param name="writer">The stream to write the record to</param>
    internal void WriteGTAModelRecord(StreamWriter writer)
    {
        WriteThenComma(writer, HouseholdId);
        WriteThenComma(writer, PersonNumber);
        WriteThenComma(writer, Age);
        WriteThenComma(writer, ConvertSex());
        WriteThenComma(writer, 'N'); // License
        WriteThenComma(writer, 'N'); // Transit Pass
        WriteThenComma(writer, ConvertEmploymentStatus());
        WriteThenComma(writer, ConvertOccupation());
        WriteThenComma(writer, 'N'); // FreeParking
        WriteThenComma(writer, ConvertStudentStatus());
        WriteThenComma(writer, EmploymentZone);
        WriteThenComma(writer, SchoolZone);
        writer.WriteLine(ExpansionFactor);
    }
}
