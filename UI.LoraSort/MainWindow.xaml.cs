/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

using Microsoft.WindowsAPICodePack.Dialogs;
using Services.LoraAutoSort;
using Services.LoraAutoSort.Classes;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
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
            // Optionally set the DataContext to this if you plan to bind CustomTagMappings directly.
            // Otherwise, add CustomTagMappings to your MainViewModel.
            this.DataContext = this;
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
        public void MoveMappingUp(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index > 0)
            {
                CustomTagMappings.Move(index, index - 1);
                UpdatePriorities();
            }
        }

        public void MoveMappingDown(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index < CustomTagMappings.Count - 1)
            {
                CustomTagMappings.Move(index, index + 1);
                UpdatePriorities();
            }
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

        private void AppendLog(string message, bool isError = false)
        {
            Dispatcher.Invoke(() =>
            {
                // Create a new paragraph for each log entry
                Paragraph paragraph = new Paragraph(new Run(message));

                // Apply color based on error status
                if (isError)
                {
                    paragraph.Foreground = Brushes.Red;
                }
                else
                {
                    paragraph.Foreground = Brushes.Black;
                }

                // Add the paragraph to the RichTextBox
                rtbLog.Document.Blocks.Add(paragraph);

                // Scroll to the end of the RichTextBox to ensure the last entry is visible
                rtbLog.ScrollToEnd();
            });
        }


        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtBasePath.Text) || String.IsNullOrEmpty(txtTargetPath.Text))
            {
                MessageBox.Show("No path selected", "No Path", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                return;
            }

            if (IsPathTheSame())
            {
                MessageBox.Show("Select a different target than the source path.", "Source cannot be targe path", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                return;
            }
            ControllerService controllerService = new ControllerService();

            if ((bool)radioCopy.IsChecked && !controllerService.EnoughFreeSpaceOnDisk(txtBasePath.Text, txtTargetPath.Text))
            {
                MessageBox.Show("You don't have enough disk space to copy the files.", "Insuficcent Diskspace", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                return;
            }

            rtbLog.Document.Blocks.Clear(); // Clear previous entries

            bool moveOperation = false;
            if (!(bool)radioCopy.IsChecked)
            {
                if (!ShowConfirmationDialog("Moving instead of copying means that the original file order cannot be restored. Continue anyways?", "Are you sure?"))
                {
                    return;
                }
                else
                {
                    moveOperation = true;
                }
            }

            List<OperationResult> results = controllerService.ComputeFolder(txtBasePath.Text, txtTargetPath.Text, moveOperation, (bool)chbOverride.IsChecked);

            if (results != null && results.Count > 0)
            {
                foreach (var result in results)
                {
                    // Call the AppendLog method, passing true if the operation was not successful
                    AppendLog(result.Message, !result.IsSuccessful);
                }
            }
            else
            {
                AppendLog("No operation results to display.");
            }
        }

        private bool IsPathTheSame()
        {
            return String.Compare(
                Path.GetFullPath(txtBasePath.Text).TrimEnd('\\'),
                Path.GetFullPath(txtTargetPath.Text).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase
            ) == 0;
        }

        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; } = new ObservableCollection<CustomTagMap>();
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