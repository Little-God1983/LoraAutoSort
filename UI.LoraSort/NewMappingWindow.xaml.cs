using Services.LoraAutoSort.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for NewMappingWindow.xaml
    /// </summary>
    public partial class NewMappingWindow : Window
    {
        // This property will hold the mapping created by the user.
        public CustomTagMap CreatedMapping { get; set; }

        public NewMappingWindow()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get the comma-separated tags and split them into a list.
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

            // Create the new mapping. (For now Priority is left as 0.)
            CreatedMapping = new CustomTagMap
            {
                LookForTag = tagsList,
                MapToFolder = folder,
                Priority = 0
            };

            // Set the dialog result and close.
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
