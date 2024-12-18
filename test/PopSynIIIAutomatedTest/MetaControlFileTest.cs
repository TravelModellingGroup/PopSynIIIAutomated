namespace PopSynIIIAutomatedTest;

[TestClass]
public class MetaControlFileTest
{
    [TestMethod]
    public void CreateMetaRecord()
    {
        Configuration config = new("TestFiles/Scenarios/TestScenario", "TestFiles", "Output", String.Empty, String.Empty, String.Empty, String.Empty);
        ZoneSystem zones = new(config);
        var tazRecords = TazControlRecord.LoadRecordsFromLines(File.ReadAllLines("TestFiles/BaseYearData/taz_controls.csv"), zones, out var additionalHeaders);
        Assert.IsNotNull(tazRecords);
        Assert.AreEqual(7, tazRecords.Length, "We read in an unexpected number of TAZ!");
        var metaRecords = MetaControlRecord.CreateMetaControlsFrom(tazRecords, additionalHeaders);
        Assert.AreEqual(2, metaRecords.Length);
        Assert.AreEqual(24, metaRecords[0].TotalHouseholds);
        Assert.AreEqual(60, metaRecords[0].TotalPopulation);
        Assert.AreEqual(60, metaRecords[1].TotalHouseholds);
        Assert.AreEqual(150, metaRecords[1].TotalPopulation);
    }
}
