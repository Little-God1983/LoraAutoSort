﻿using Microsoft.WindowsAPICodePack.Dialogs;
using Services.LoraAutoSort;
using Services.LoraAutoSort.Classes;
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

        //private void btnGo_Click(object sender, RoutedEventArgs e)
        //{

        //    txtLog.Clear();
        //    ControllerService controllerService = new ControllerService();
        //    if ((bool)radioCopy.IsChecked && ShowConfirmationDialog("Moving instead of copying means that the original file order cannot be restored. Continue anyways?", "Are you sure?"))
        //    {
        //        return;
        //    }

        //    List<OperationResult> results = controllerService.ComputeFolder(txtBasePath.Text, txtTargetPath.Text, false);
        //    // Check if there are any results to display
        //    if (results != null && results.Count > 0)
        //    {
        //        foreach (var result in results)
        //        {
        //            // Append each message to the TextBox, each on a new line
        //            txtLog.AppendText(result.Message + Environment.NewLine);
        //        }
        //    }
        //    else
        //    {
        //        txtLog.AppendText("No operation results to display.");
        //    }




        //}

        private void AppendLog(string message, bool isError = false)
        {
            // Dispatcher.Invoke is used to ensure thread safety when accessing the UI thread from another thread
            Dispatcher.Invoke(() =>
            {
                TextRange rangeOfText = new TextRange(rtbLog.Document.ContentEnd, rtbLog.Document.ContentEnd);
                rangeOfText.Text = message + Environment.NewLine;

                if (isError)
                {
                    // Set the foreground color to red for errors
                    rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                }
                else
                {
                    // Set the foreground color to black for normal messages
                    rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                }

                // Scroll to the end of the RichTextBox to ensure the last entry is visible
                rtbLog.ScrollToEnd();
            });
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            rtbLog.Document.Blocks.Clear(); // Clear previous entries
            ControllerService controllerService = new ControllerService();

            List<OperationResult> results = controllerService.ComputeFolder(txtBasePath.Text, txtTargetPath.Text, false);

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
    }
}