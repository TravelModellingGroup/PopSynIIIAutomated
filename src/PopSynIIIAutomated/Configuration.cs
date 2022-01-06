using System;
using System.IO;
using System.Text.Json;

namespace PopSynIIIAutomated;

/// <summary>
/// This class contains all of the configuration
/// information required to complete the synthesis.
/// </summary>
/// <param name="ScenarioDirectory">The directory containing the inputs for the forecast.</param>
/// <param name="InputDirectory">The base directory of the PopSynIII instance</param>
/// <param name="OutputDirectory">The directory that we will need to write the final outputs into.</param>
/// <param name="DatabaseName">The name of the database that will be used</param>
/// <param name="DatabaseUsername">The name of the user for the database</param>
/// <param name="DatabasePassword">The password for the database user</param>
/// <param name="JavaDirectory">The location of the Java Runtime Environment installation</param>
internal record class Configuration
    (
        string ScenarioDirectory,
        string InputDirectory,
        string OutputDirectory,
        string DatabaseName,
        string DatabaseUsername,
        string DatabasePassword,
        string JavaDirectory
    )
{
    /// <summary>
    /// Load the configuration from a given file name.
    /// </summary>
    /// <param name="filePath">The path to the configuration file to load.</param>
    /// <returns>The loaded configuration file or a default configuration.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Configuration Load(string filePath)
    {
        try
        {
            return JsonSerializer.Deserialize<Configuration>(File.ReadAllText(filePath)) ?? DefaultConfiguration;
        }
        catch
        {
            return DefaultConfiguration;
        }
    }

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    internal static Configuration DefaultConfiguration => new(string.Empty, string.Empty, string.Empty,
        "TorontoPopSynIII", "root", string.Empty, "C:\\Program Files\\Java\\jre7");

    /// <summary>
    /// Save the configuration to file.
    /// </summary>
    /// <param name="filePath">The location to save the file to.</param>
    /// <exception cref="System.IO.IOException">Thrown when we are unable to write to the file.</exception>
    public void Save(string filePath)
    {
        _ = UtilityFunctions.CreateDirectories(filePath);
        File.WriteAllText(filePath, JsonSerializer.Serialize(this, typeof(Configuration), new JsonSerializerOptions()
        {
            WriteIndented = true
        }));
    }
}
