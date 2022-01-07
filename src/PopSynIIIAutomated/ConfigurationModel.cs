using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PopSynIIIAutomated;

/// <summary>
/// Provides a view model for the configuration
/// </summary>
internal class ConfigurationModel : INotifyPropertyChanged
{
    /// <summary>
    /// The parameters as they were at the last save.
    /// </summary>
    private Configuration _configuration;

    /// <summary>
    /// Create a new view model for the configuration.
    /// </summary>
    /// <param name="configuration">The configuration to base this on.</param>
    internal ConfigurationModel(Configuration configuration)
    {
        _configuration = configuration;
        _scenarioDirectory = _configuration.ScenarioDirectory;
        _inputDirectory = _configuration.InputDirectory;
        _outputDirectory = _configuration.OutputDirectory;
        _databaseName = _configuration.DatabaseName;
        _databaseUsername = _configuration.DatabaseUsername;
        _databasePassword = _configuration.DatabasePassword;
        _javaDirectory = _configuration.JavaDirectory;
    }

    internal void SetConfiguration(Configuration configuration)
    {
        _configuration = configuration;
        ScenarioDirectory = _configuration.ScenarioDirectory;
        InputDirectory = _configuration.InputDirectory;
        OutputDirectory = _configuration.OutputDirectory;
        DatabaseName = _configuration.DatabaseName;
        DatabaseUsername = _configuration.DatabaseUsername;
        DatabasePassword = _configuration.DatabasePassword;
        JavaDirectory = _configuration.JavaDirectory;
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Save the configuration to the given file path.
    /// </summary>
    /// <param name="filePath"></param>
    internal void Save(string filePath)
    {
        _configuration = GetConfiguration();
        _configuration.Save(filePath);
    }

    /// <summary>
    /// Get a copy of the current configuration.
    /// </summary>
    /// <returns>A configuration with the current values.</returns>
    internal Configuration GetConfiguration()
    {
        return new Configuration(
            ScenarioDirectory,
            InputDirectory,
            OutputDirectory,
            DatabaseName,
            DatabaseUsername,
            DatabasePassword,
            JavaDirectory
            );
    }

    /// <summary>
    /// The backing for our ScenarioDirectory.
    /// </summary>
    private string _scenarioDirectory;

    /// <summary>
    /// The folder that contains the inputs for our current run.
    /// </summary>
    public string ScenarioDirectory
    {
        get => _scenarioDirectory;
        set
        {
            _scenarioDirectory = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The backing for our InputDirectory
    /// </summary>
    private string _inputDirectory;

    /// <summary>
    /// The folder that contains the instance of PopSynIII
    /// </summary>
    public string InputDirectory
    {
        get => _inputDirectory;
        set
        {
            _inputDirectory = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The backing for our OutputDirectory
    /// </summary>
    private string _outputDirectory;

    /// <summary>
    /// The directory that we are going to write our results to in addition to saving
    /// the controls used.
    /// </summary>
    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            _outputDirectory = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The backing for DatabaseName
    /// </summary>
    private string _databaseName;

    /// <summary>
    /// The name of the database to connect to.
    /// </summary>
    public string DatabaseName
    {
        get => _databaseName;
        set
        {
            _databaseName = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The backing for DatabaseUserName
    /// </summary>
    private string _databaseUsername;

    /// <summary>
    /// The name of the user name to connect to the database with.
    /// </summary>
    public string DatabaseUsername
    {
        get => _databaseUsername;
        set
        {
            _databaseUsername = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The backing for DatabasePassword
    /// </summary>
    private string _databasePassword;

    /// <summary>
    /// The password for the database user to connect to the database with.
    /// </summary>
    public string DatabasePassword
    {
        get => _databasePassword;
        set
        {
            _databasePassword = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The backing for JavaDirectory
    /// </summary>
    private string _javaDirectory;

    /// <summary>
    /// The location that the JRE was installed to.
    /// </summary>
    public string JavaDirectory
    {
        get => _javaDirectory;
        set
        {
            _javaDirectory = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Used to automate the notification that a property was changed.
    /// </summary>
    /// <param name="callingName">The name of the member that invoked this method.</param>
    private void NotifyPropertyChanged([CallerMemberName] string callingName = "")
    {
        PropertyChanged?.Invoke(callingName, new PropertyChangedEventArgs(callingName));
    }
}
