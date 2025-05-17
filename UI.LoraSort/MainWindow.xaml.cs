using System;
using System.Windows;
using UI.LoraSort.ViewModels;

namespace UI.LoraSort
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                // Save dlg.ApiKey to settings or ViewModel as needed
                // Example: Properties.Settings.Default.CivitAiApiKey = dlg.ApiKey;
                // Properties.Settings.Default.Save();
            }
        }
    }
}
