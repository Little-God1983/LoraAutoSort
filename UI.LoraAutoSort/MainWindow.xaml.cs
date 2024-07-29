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
                    txtTargetPath.Text = dialog.FileName; // Retrieves the selected path
                }
            }
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
           ControllerService controllerService = new ControllerService();
           List<OperationResult> test =  controllerService.ComputeFolder(txtBasePath.Text, txtTargetPath.Text);

        }
    }
}