using Microsoft.WindowsAPICodePack.Dialogs;
using Services.LoraAutoSort;
using Services.LoraAutoSort.Classes;
using System.Windows;

namespace UI.LoraAutoSort
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
                    //txtTargetPath.Text = dialog.FileName; // Retrieves the selected path
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

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            chbOverride.
           txtLog.Clear();
           ControllerService controllerService = new ControllerService();
            if (radioMove == IsActive && ShowConfirmationDialog("Moving instead of copying means that the original file order cannot be restored", "Are you sure?"))
            {

            }

            List<OperationResult> results = controllerService.ComputeFolder(txtBasePath.Text, txtTargetPath.Text, false);
            // Check if there are any results to display
            if (results != null && results.Count > 0)
            {
                foreach (var result in results)
                {
                    // Append each message to the TextBox, each on a new line
                    txtLog.AppendText(result.Message + Environment.NewLine);
                }
            }
            else
            {
               txtLog.AppendText("No operation results to display.");
            }
        }
    }
}