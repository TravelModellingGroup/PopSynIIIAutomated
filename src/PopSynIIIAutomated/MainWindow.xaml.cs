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
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Run_Clicked(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            var runTask = Run_Async();
            try
            {
                await runTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            this.IsEnabled = true;
        }

        private Task Run_Async()
        {
            return Task.Run(() =>
            {
                Runtime.RunPreprocessor(new Configuration("Scenario", ".", "OutputDirectory", String.Empty, String.Empty, String.Empty));
            });
        }
    }
}
