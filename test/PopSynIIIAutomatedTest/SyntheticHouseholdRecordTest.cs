
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace PopSynIIIAutomatedTest;

/// <summary>
/// Tests the operations for working with the records that have
/// been generated from PopSynIII and their conversion for GTAModel.
/// </summary>
[TestClass]
public class SyntheticHouseholdRecordTest
{
    /// <summary>
    /// Check that all of the records have been correctly loaded from file.
    /// </summary>
    [TestMethod]
    public void LoadRecordsFromLines()
    {
        var records = SyntheticHouseholdRecord.LoadRecords("TestFiles/Output/SyntheticHouseholds.csv");
        Assert.AreEqual(3, records.Length);
        // Check household numbers
        Assert.AreEqual(1, records[0].HouseholdId);
        Assert.AreEqual(2, records[1].HouseholdId);
        Assert.AreEqual(3, records[2].HouseholdId);

        // Check TAZ
        Assert.AreEqual(1, records[0].TAZ);
        Assert.AreEqual(2, records[1].TAZ);
        Assert.AreEqual(6, records[2].TAZ);

        // Check Expansion Factor
        Assert.AreEqual(1, records[0].ExpansionFactor);
        Assert.AreEqual(2, records[1].ExpansionFactor);
        Assert.AreEqual(3, records[2].ExpansionFactor);

        // Dwelling type
        Assert.AreEqual(1, records[0].DwellingType);
        Assert.AreEqual(2, records[1].DwellingType);
        Assert.AreEqual(1, records[2].DwellingType);

        // Number of Persons
        Assert.AreEqual(1, records[0].NumberOfPersons);
        Assert.AreEqual(2, records[1].NumberOfPersons);
        Assert.AreEqual(4, records[2].NumberOfPersons);

        // Vehicles
        Assert.AreEqual(3, records[0].Vehicles);
        Assert.AreEqual(4, records[1].Vehicles);
        Assert.AreEqual(5, records[2].Vehicles);

        // Income Class
        Assert.AreEqual(2, records[0].IncomeClass);
        Assert.AreEqual(3, records[1].IncomeClass);
        Assert.AreEqual(4, records[2].IncomeClass);
    }

    [TestMethod]
    public void WriteHeader()
    {
        string expected = "HouseholdId,HouseholdZone,ExpansionFactor,DwellingType,NumberOfPersons,Vehicles,IncomeClass\r\n";
        var result = Utilities.GetStreamResult(writer =>
        {
            SyntheticHouseholdRecord.WriteGTAModelHeader(writer);
        });
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WriteRecord()
    {
        string[] expected =
            {
                "1,1,1,1,1,3,2\r\n",
                "2,2,2,2,2,4,3\r\n",
                "3,6,3,1,4,5,4\r\n",
            };
        var records = SyntheticHouseholdRecord.LoadRecords("TestFiles/Output/SyntheticHouseholds.csv");
        Assert.AreEqual(3, records.Length);
        for (int i = 0; i < records.Length; i++)
        {
            var result = Utilities.GetStreamResult(writer =>
            {
                records[i].WriteGTAModelRecord(writer);
            });
            Assert.AreEqual(expected[i], result);
        }
    }
}
