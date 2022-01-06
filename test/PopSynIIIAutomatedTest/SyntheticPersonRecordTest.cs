using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace PopSynIIIAutomatedTest;

/// <summary>
/// Test the functionality of reading and writing converted records from
/// PopSynIII to GTAModel.
/// </summary>
[TestClass]
public class SyntheticPersonRecordTest
{
    /// <summary>
    /// Check that we have correctly read in the test records
    /// </summary>
    [TestMethod]
    public void LoadRecords()
    {
        var records = SyntheticPersonRecord.LoadRecords("TestFiles/Output/SyntheticPersons.csv");
        Assert.AreEqual(7, records.Length);

        // Check Household TAZ
        Assert.AreEqual(1, records[0].TAZ);
        Assert.AreEqual(2, records[1].TAZ);
        Assert.AreEqual(2, records[2].TAZ);

        // Check householdId
        Assert.AreEqual(1, records[0].HouseholdId);
        Assert.AreEqual(2, records[1].HouseholdId);
        Assert.AreEqual(2, records[2].HouseholdId);

        // Check Person Number
        Assert.AreEqual(1, records[0].PersonNumber);
        Assert.AreEqual(1, records[1].PersonNumber);
        Assert.AreEqual(2, records[2].PersonNumber);

        // Check Age
        Assert.AreEqual(24, records[0].Age);
        Assert.AreEqual(36, records[1].Age);
        Assert.AreEqual(35, records[2].Age);

        // Check Sex
        Assert.AreEqual(2, records[0].Sex);
        Assert.AreEqual(1, records[1].Sex);
        Assert.AreEqual(2, records[2].Sex);

        // Check Employment Status
        Assert.AreEqual(2, records[0].EmploymentStatus);
        Assert.AreEqual(3, records[1].EmploymentStatus);
        Assert.AreEqual(2, records[2].EmploymentStatus);

        // Check Occupation
        Assert.AreEqual(1, records[0].Occupation);
        Assert.AreEqual(2, records[1].Occupation);
        Assert.AreEqual(3, records[2].Occupation);

        // Check Student Status
        Assert.AreEqual(2, records[0].StudentStatus);
        Assert.AreEqual(1, records[1].StudentStatus);
        Assert.AreEqual(3, records[2].StudentStatus);

        // Check Employment Zone
        Assert.AreEqual(0, records[0].EmploymentZone);
        Assert.AreEqual(0, records[1].EmploymentZone);
        Assert.AreEqual(6000, records[4].EmploymentZone);

        // Check School Zone
        Assert.AreEqual(0, records[0].SchoolZone);
        Assert.AreEqual(1, records[5].SchoolZone);
        Assert.AreEqual(2, records[6].SchoolZone);

        // Check Expansion Factor
        Assert.AreEqual(1.0f, records[0].ExpansionFactor);
        Assert.AreEqual(2.0f, records[1].ExpansionFactor);
        Assert.AreEqual(2.0f, records[2].ExpansionFactor);
    }

    /// <summary>
    /// The that we have written the loaded records correctly.
    /// </summary>
    [TestMethod]
    public void WriteRecords()
    {
        var records = SyntheticPersonRecord.LoadRecords("TestFiles/Output/SyntheticPersons.csv");
        Assert.AreEqual(7, records.Length);

        var expected = new string[]
        {
            "1,1,24,M,N,N,F,G,N,P,0,0,1\r\n",
            "2,1,36,F,N,N,P,S,N,F,0,0,2\r\n",    
            "2,2,35,M,N,N,F,M,N,O,0,0,2\r\n",
            "3,1,41,M,N,N,F,P,N,O,0,0,3\r\n",
            "3,2,40,F,N,N,F,G,N,O,6000,0,3\r\n", 
            "3,3,9,F,N,N,O,O,N,F,0,1,3\r\n",
            "3,4,6,M,N,N,O,O,N,F,0,2,3\r\n"
        };                                 

        for (int i = 0; i < records.Length; i++)
        {
            var result = Utilities.GetStreamResult((writer) =>
            {
                records[i].WriteGTAModelRecord(writer);
            });
            Assert.AreEqual(expected[i], result);
        }
    }
}
