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
            AppSettingsUC.CustomMappingsCheckedChanged += AppSettingsUC_CustomMappingsCheckedChanged;
        }

        private void AppSettingsUC_CustomMappingsCheckedChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            CustomMappingsUC.IsCustomEnabled = e.NewValue;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                // Save encrypted API key to settings
                SettingsManager.SaveApiKey(dlg.ApiKey);
            }
        }
    }
}
