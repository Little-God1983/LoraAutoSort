using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Services.LoraAutoSort.Services;
using Services.LoraAutoSort.Classes;
using UI.LoraSort.ViewModels;
using Serilog;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for AppSettingsTopLeftControl.xaml
    /// </summary>
    public partial class AppSettingsTopLeftControl : UserControl
    {
        private CancellationTokenSource _cts;
        private bool _isProcessing = false;

        public AppSettingsTopLeftControl()
        {
            InitializeComponent();
        }

        private void SelectBasePath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Select the base path";
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtBasePath.Text = dialog.FileName;
                }
            }
        }

        private void SelectTargetPath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Select the target path";
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtTargetPath.Text = dialog.FileName;
                }
            }
        }

        private async void btnGoCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!_isProcessing)
            {
                await StartProcessingAsync();
            }
            else
            {
                _cts?.Cancel();
            }
        }

        private async Task StartProcessingAsync()
        {
            try
            {
                SetProcessingUIState();

                if (!ValidatePaths())
                {
                    ShowMessageAndResetUI("No path selected", "No Path");
                    return;
                }

                if (IsPathTheSame())
                {
                    ShowMessageAndResetUI("Select a different target than the source path.", "Source cannot be target path");
                    return;
                }

                var controllerService = new FileControllerService();
                if ((bool)radioCopy.IsChecked && !controllerService.EnoughFreeSpaceOnDisk(txtBasePath.Text, txtTargetPath.Text))
                {
                    ShowMessageAndResetUI("You don't have enough disk space to copy the files.", "Insuficcent Diskspace");
                    return;
                }

                bool moveOperation = false;
                if (!(bool)radioCopy.IsChecked)
                {
                    if (!ShowConfirmationDialog("Moving instead of copying means that the original file order cannot be restored. Continue anyways?", "Are you sure?"))
                    {
                        ResetUI();
                        return;
                    }
                    moveOperation = true;
                }

                var progressIndicator = CreateProgressIndicator();
                await controllerService.ComputeFolder(progressIndicator, _cts.Token, new SelectedOptions()
                {
                    BasePath = txtBasePath.Text,
                    TargetPath = txtTargetPath.Text,
                    IsMoveOperation = moveOperation,
                    OverrideFiles = (bool)chbOverride.IsChecked,
                    CreateBaseFolders = (bool)chbBaseFolders.IsChecked,
                    UseCustomMappings = (bool)chbCustom.IsChecked
                });
            }
            catch (OperationCanceledException)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.AppendLog("Operation was canceled by user.", isError: false);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected error: {ex.Message}");
            }
            finally
            {
                ResetUI();
            }
        }

        private bool ValidatePaths()
        {
            return !string.IsNullOrEmpty(txtBasePath.Text) && !string.IsNullOrEmpty(txtTargetPath.Text);
        }

        private void ShowMessageAndResetUI(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
            ResetUI();
        }

        private void SetProcessingUIState()
        {
            _isProcessing = true;
            btnGoCancel.Content = "Cancel";
            if (DataContext is MainViewModel vm)
            {
                vm.ClearLogs();
            }
            btnTargetPath.IsEnabled = false;
            btnBasePath.IsEnabled = false;
            _cts = new CancellationTokenSource();
        }

        private IProgress<ProgressReport> CreateProgressIndicator()
        {
            return new Progress<ProgressReport>(report =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (report.Percentage.HasValue)
                    {
                        progressBar.Value = report.Percentage.Value;
                    }
                    txtStatus.Text = report.StatusMessage;
                    if (DataContext is MainViewModel vm)
                    {
                        vm.LogEntries.Add(report);
                    }
                });
            });
        }

        private void ResetUI()
        {
            _isProcessing = false;
            btnTargetPath.IsEnabled = true;
            btnBasePath.IsEnabled = true;
            btnGoCancel.Content = "Go";
        }

        private bool IsPathTheSame()
        {
            return string.Compare(
                Path.GetFullPath(txtBasePath.Text).TrimEnd('\\'),
                Path.GetFullPath(txtTargetPath.Text).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private bool ShowConfirmationDialog(string message, string title)
        {
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
