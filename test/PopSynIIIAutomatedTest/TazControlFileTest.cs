using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace PopSynIIIAutomatedTest;

/// <summary>
/// Used for testing the loading and creation of control totals for TAZ
/// </summary>
[TestClass]
public class TazControlFileTest
{
    /// <summary>
    /// Test that we are able to load in the TAZ controls properly
    /// </summary>
    [TestMethod]
    public void LoadInControls()
    {
        var lines = File.ReadAllLines("TestFiles/BaseYearData/taz_controls.csv");
        Assert.AreEqual(8, lines.Length, "Our test file had an unexpected number of lines!");
        var records = TazControlRecord.LoadRecordsFromLines(lines);
        Assert.IsNotNull(records);
        Assert.AreEqual(7, records.Length);
        // The last record has a zero population
        for (int i = 0; i < records.Length - 1; i++)
        {
            Assert.AreEqual(i + 1, records[i].TAZ);
            Assert.AreEqual((i + 1) * 10, records[i].TotalPopulation);
            Assert.AreEqual((i + 1) * 4, records[i].TotalHouseholds);
        }
    }

    /// <summary>
    /// Test that we are able to do a basic forecast on the TAZ controls.
    /// </summary>
    [TestMethod]
    public void Forecast()
    {
        var outputPath = "Output/taz_controls.csv";
        DeleteIfFileExists(outputPath);
        Configuration config = new("TestFiles/Scenarios/TestScenario", "TestFiles", "Output", String.Empty, String.Empty, String.Empty, String.Empty);
        ZoneSystem zones = new(config);
        // Tell it to create a new taz control file where all of the zones should be the same.
        TazControlFile.CreateForecastControls(config, zones, new Dictionary<int, float>()
        {
            { 1, 10.0f },
            { 2, 10.0f },
            { 3, 10.0f },
            { 4, 10.0f },
            { 5, 10.0f },
            { 6, 10.0f },
            { 7, 10.0f }
        });
        Assert.IsTrue(File.Exists(outputPath));
        var records = TazControlRecord.LoadRecordsFromLines(File.ReadAllLines(outputPath));
        Assert.IsNotNull(records);
        Assert.AreEqual(7, records.Length);
        for (int i = 0; i < records.Length; i++)
        {
            Assert.AreEqual(i + 1, records[i].TAZ);
            Assert.AreEqual(4, records[i].TotalHouseholds);
            Assert.AreEqual(10, records[i].TotalPopulation);
        }
    }
}
