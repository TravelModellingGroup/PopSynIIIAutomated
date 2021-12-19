namespace PopSynIIIAutomatedTest;

using static PopSynIIIAutomated.UtilityFunctions;

/// <summary>
/// This class will test the helper functions contained in
/// PopSynIIIAutomated.UtilityFunctions.
/// </summary>
[TestClass]
public class UtilityFunctionsTest
{
    /// <summary>
    /// Tests the Scale function
    /// </summary>
    [TestMethod]
    public void TestScale()
    {
        Assert.AreEqual(21, Scale(10, 2.1f));
        Assert.AreEqual(21, Scale(10, 2.14f));
        Assert.AreEqual(22, Scale(10, 2.15f));
    }

    /// <summary>
    /// Tests WriteComma without the scale parameter
    /// </summary>
    [TestMethod]
    public void TestWriteCommaNoScale()
    {
        Assert.AreEqual("10,", GetStreamResult(writer => WriteThenComma(writer, 10)));
        Assert.AreEqual("20,", GetStreamResult(writer => WriteThenComma(writer, 20)));
    }

    /// <summary>
    /// Tests WriteComma with the scale parameter
    /// </summary>
    [TestMethod]
    public void TestWriteCommaScale()
    {
        Assert.AreEqual("21,", GetStreamResult(writer => WriteThenComma(writer, 10, 2.1f)));
        Assert.AreEqual("21,", GetStreamResult(writer => WriteThenComma(writer, 10, 2.14f)));
        Assert.AreEqual("22,", GetStreamResult(writer => WriteThenComma(writer, 10, 2.15f)));
    }
}
