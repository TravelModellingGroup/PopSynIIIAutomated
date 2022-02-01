using System;
using System.Threading.Tasks;
using System.Windows;

namespace PopSynIIIAutomated
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ConfigurationFilePath = "config.json";
        private static string RunBoxText = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            IsEnabled = false;
            DataContext = new ConfigurationModel(Configuration.DefaultConfiguration);
            // Setup the runtime to dump out to a message box
            Runtime.DisplayToUser = (message) =>
            {
                RunBoxText = string.Concat(RunBoxText, "\n",  message);
                Dispatcher.Invoke(() =>
                {
                    RunTextBox.Text = RunBoxText;
                });
            };
            Task.Run(() =>
            {
                var config = Configuration.Load(ConfigurationFilePath);
                Dispatcher.Invoke(() =>
                {
                    (DataContext as ConfigurationModel)?.SetConfiguration(config);
                });
            }).ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    ShowErrorMessage((task.Exception?.Message ?? "") + "\r\n" + (task.Exception?.StackTrace ?? ""));
                }
                Dispatcher.Invoke(() =>
                {
                    IsEnabled = true;
                });
            });
        }

        private async void Run_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                await RunAllAsync((DataContext as ConfigurationModel)?.GetConfiguration() ?? Configuration.DefaultConfiguration);
            }
            catch (Exception ex)
            {
                ShowErrorMessage((ex.Message ?? "") + "\r\n" + ex.StackTrace);
            }
        }

        private void ShowErrorMessage(string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(this, errorMessage);
            });
        }

        private Task RunAllAsync(Configuration config)
        {
            config.Save(ConfigurationFilePath);
            return Task.Run(() =>
            {
                Runtime.RunAll(config);
            });
        }

        private Task RunPreProcessorAsync(Configuration config)
        {
            config.Save(ConfigurationFilePath);
            return Task.Run(() =>
            {
                Runtime.RunPreprocessor(config);
            });
        }

        private Task RunPostProcessorAsync(Configuration config)
        {
            config.Save(ConfigurationFilePath);
            return Task.Run(() =>
            {
                Runtime.RunPostProcessor(config);
            });
        }

        private async void RunPreprocessor_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                await RunPreProcessorAsync((DataContext as ConfigurationModel)?.GetConfiguration() ?? Configuration.DefaultConfiguration);
            }
            catch (Exception ex)
            {
                ShowErrorMessage((ex.Message ?? "") + "\r\n" + ex.StackTrace);
            }
        }

        private async void RunPostprocessor_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                await RunPostProcessorAsync((DataContext as ConfigurationModel)?.GetConfiguration() ?? Configuration.DefaultConfiguration);
            }
            catch (Exception ex)
            {
                ShowErrorMessage((ex.Message ?? "") + "\r\n" + ex.StackTrace);
            }
        }
    }
}
