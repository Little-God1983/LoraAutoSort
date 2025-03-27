/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */
using Microsoft.WindowsAPICodePack.Dialogs;
using Services.LoraAutoSort.Classes;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UI.LoraSort.ViewModels;
using Serilog;
using Services.LoraAutoSort.Services;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;
        private bool _isProcessing = false;

        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; }
        // ICommand properties for moving items
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            // Optionally set the DataContext to this if you plan to bind CustomTagMappings directly.
            // Otherwise, add CustomTagMappings to your MainViewModel.
            CustomTagMappings = LoadMapping();

            // Initialize commands. 
            MoveUpCommand = new RelayCommand<CustomTagMap>(MoveMappingUp, CanMoveUp);
            MoveDownCommand = new RelayCommand<CustomTagMap>(MoveMappingDown, CanMoveDown);

            var vm = new MainViewModel();
            vm.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
        }
        private void LogEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && logListView.Items.Count > 0)
            {
                // Use Dispatcher to ensure the ListView has updated its items.
                logListView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    logListView.ScrollIntoView(logListView.Items[logListView.Items.Count - 1]);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private ObservableCollection<CustomTagMap> LoadMapping()
        {
            CustomTagMapXmlService xmlService = new CustomTagMapXmlService();
            return xmlService.LoadMappings();
        }


        /// <summary>
        /// Opens the NewMappingWindow to create a new mapping.
        /// </summary>
        private void btnAddMapping_Click(object sender, RoutedEventArgs e)
        {
            NewMappingWindow newMappingWindow = new NewMappingWindow();
            bool? result = newMappingWindow.ShowDialog();

            if (result == true && newMappingWindow.CreatedMapping != null)
            {
                // Add the new mapping to your collection.
                CustomTagMappings.Add(newMappingWindow.CreatedMapping);
                // Optionally, you can update priorities here.
                UpdatePriorities();
            }
        }
        /// <summary>
        /// Deletes the selected mapping from the ListView.
        /// </summary>
        private void btnDeleteMapping_Click(object sender, RoutedEventArgs e)
        {
            if (lvMappings.SelectedItem is CustomTagMap mapping)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected mapping?",
                                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CustomTagMappings.Remove(mapping);
                    UpdatePriorities();
                }
            }
            else
            {
                MessageBox.Show("Please select a mapping to delete.", "No Mapping Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // (Existing methods MoveMappingUp, MoveMappingDown, UpdatePriorities, etc.)
        // Method called by MoveUpCommand.
        public void MoveMappingUp(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index > 0)
            {
                CustomTagMappings.Move(index, index - 1);
                // No need to update a separate priority property now.
                CollectionViewSource.GetDefaultView(CustomTagMappings).Refresh();
            }
        }

        public void MoveMappingDown(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index < CustomTagMappings.Count - 1)
            {
                CustomTagMappings.Move(index, index + 1);
                // No need to update a separate priority property now.
                CollectionViewSource.GetDefaultView(CustomTagMappings).Refresh();
            }
        }

        // Optional: Implement the CanExecute predicates.
        private bool CanMoveUp(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            return index > 0;
        }

        private bool CanMoveDown(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            return index < CustomTagMappings.Count - 1;
        }

        private void UpdatePriorities()
        {
            for (int i = 0; i < CustomTagMappings.Count; i++)
            {
                CustomTagMappings[i].Priority = i;
            }
        }

        public MainViewModel ViewModel { get; set; }
        private void SelectBasePath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true; // Important: Makes it a folder picker
                dialog.Title = "Select the base path"; // Sets the title for the dialog

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtBasePath.Text = dialog.FileName; // Sets the selected folder path to the text box
                }
            }
        }

        private void SelectTargetPath(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true; // Configures the dialog to select folders
                dialog.Title = "Select the target path";

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtTargetPath.Text = dialog.FileName; // Retrieves the selected path
                }
            }
        }
        private bool ShowConfirmationDialog(string message, string title)
        {
            // Configure the message box to be displayed
            MessageBoxResult result = MessageBox.Show(message, title,
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            // Check the user's response and return true for Yes, false for No
            return result == MessageBoxResult.Yes;
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
                    CreateBaseFolders = (bool)chbBaseFolders.IsChecked
                });
            }
            catch (OperationCanceledException)
            {
                ((MainViewModel)DataContext).AppendLog("Operation was canceled by user.", isError: false);
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
            ((MainViewModel)DataContext).ClearLogs();
            btnTargetPath.IsEnabled = false;
            btnBasePath.IsEnabled = false;
            _cts = new CancellationTokenSource();
        }

        private IProgress<ProgressReport> CreateProgressIndicator()
        {
            return new Progress<ProgressReport>(report =>
            {
                // Ensure UI updates are on the UI thread.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (report.Percentage.HasValue)
                    {
                        progressBar.Value = report.Percentage.Value;
                    }
                    txtStatus.Text = report.StatusMessage;
                    ((MainViewModel)DataContext)?.LogEntries.Add(report);
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
            return String.Compare(
                Path.GetFullPath(txtBasePath.Text).TrimEnd('\\'),
                Path.GetFullPath(txtTargetPath.Text).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }


        private void btnEditMapping_Click(object sender, RoutedEventArgs e)
        {
            // Get the mapping from the Button's CommandParameter.
            if (((FrameworkElement)sender).DataContext is CustomTagMap mapping)
            {
                EditMappingWindow editWindow = new EditMappingWindow(mapping)
                {
                    Owner = this
                };
                bool? result = editWindow.ShowDialog();
                if (result == true)
                {
                    // If your CustomTagMap implements INotifyPropertyChanged,
                    // the ListView will update automatically. Otherwise, force a refresh:
                    lvMappings.Items.Refresh();
                }
            }
        }

        private void btnSaveMapping_Click(object sender, RoutedEventArgs e)
        {
            CustomTagMapXmlService xmlService = new CustomTagMapXmlService();
            xmlService.SaveMappings(CustomTagMappings);
        }

        private void btnDeleteAllMappings_Click(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation.
            MessageBoxResult result = MessageBox.Show("Do you really want to delete all mappings from disk?",
                                                      "Confirm Delete",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Define the path where the mappings are stored.
                    string filePath = "mappings.xml";  // Replace with your actual file path

                    // Create an instance of the service and call the delete method.
                    CustomTagMapXmlService service = new CustomTagMapXmlService();
                    service.DeleteAllMappings(filePath);

                    // Optionally clear the ObservableCollection if you're also showing it in the UI.
                    CustomTagMappings.Clear();

                    MessageBox.Show("All mappings have been deleted.", "Deletion Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting mappings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}