/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

using Microsoft.WindowsAPICodePack.Dialogs;
using Services.LoraAutoSort;
using Services.LoraAutoSort.Classes;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

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
            if(IsPathTheSame())
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
    }
}