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

        public MainWindow()
        {
            InitializeComponent();
            IsEnabled = false;
            DataContext = new ConfigurationModel(Configuration.DefaultConfiguration);
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
                    this.IsEnabled = true;
                });
            });
        }

        private async void Run_Clicked(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            try
            {
                await RunAllAsync((DataContext as ConfigurationModel)?.GetConfiguration() ?? Configuration.DefaultConfiguration);
            }
            catch (Exception ex)
            {
                ShowErrorMessage((ex.Message ?? "") + "\r\n" + ex.StackTrace);
            }
            this.IsEnabled = true;
        }

        private void ShowErrorMessage(string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(errorMessage);
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
            this.IsEnabled = false;
            try
            {
                await RunPreProcessorAsync((DataContext as ConfigurationModel)?.GetConfiguration() ?? Configuration.DefaultConfiguration);
            }
            catch (Exception ex)
            {
                ShowErrorMessage((ex.Message ?? "") + "\r\n" + ex.StackTrace);
            }
            this.IsEnabled = true;
        }

        private async void RunPostprocessor_Clicked(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            try
            {
                await RunPostProcessorAsync((DataContext as ConfigurationModel)?.GetConfiguration() ?? Configuration.DefaultConfiguration);
            }
            catch (Exception ex)
            {
                ShowErrorMessage((ex.Message ?? "") + "\r\n" + ex.StackTrace);
            }
            this.IsEnabled = true;
        }
    }
}
