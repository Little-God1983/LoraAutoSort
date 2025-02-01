using Services.LoraAutoSort.Classes;
using System.Windows;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for EditMappingWindow.xaml
    /// </summary>
    public partial class EditMappingWindow : Window
    {
        // Holds the mapping that is being edited.
        public CustomTagMap Mapping { get; set; }

        public EditMappingWindow(CustomTagMap mapping)
        {
            InitializeComponent();
            Mapping = mapping;
            // Prepopulate the fields with the mapping's data.
            txtTags.Text = string.Join(", ", Mapping.LookForTag);
            txtFolder.Text = Mapping.MapToFolder;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get the comma–separated tags.
            string tagsInput = txtTags.Text.Trim();
            List<string> tagsList = new List<string>();
            if (!string.IsNullOrWhiteSpace(tagsInput))
            {
                tagsList = tagsInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList();
            }

            // Get the target folder.
            string folder = txtFolder.Text.Trim();
            if (tagsList.Count == 0 || string.IsNullOrEmpty(folder))
            {
                MessageBox.Show("Please enter at least one tag and a target folder.", "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update the mapping.
            Mapping.LookForTag = tagsList;
            Mapping.MapToFolder = folder;

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
