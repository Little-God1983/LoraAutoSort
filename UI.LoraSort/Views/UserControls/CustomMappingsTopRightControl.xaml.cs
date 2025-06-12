using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Services.LoraAutoSort.Classes;
using Services.LoraAutoSort.Services;
using UI.LoraSort.Infrastructure;
using UI.LoraSort.Views.Dialog;

namespace UI.LoraSort.Views.UserControls
{
    /// <summary>
    /// Interaction logic for CustomMappingsTopRightControl.xaml
    /// </summary>
    public partial class CustomMappingsTopRightControl : UserControl
    {
        // Collection bound to the ListView.
        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; }

        // Commands for moving items up and down.
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }

        // DependencyProperty for IsCustomEnabled.
        public static readonly DependencyProperty IsCustomEnabledProperty = DependencyProperty.Register(
            nameof(IsCustomEnabled), typeof(bool), typeof(CustomMappingsTopRightControl), new PropertyMetadata(true));

        public bool IsCustomEnabled
        {
            get => (bool)GetValue(IsCustomEnabledProperty);
            set => SetValue(IsCustomEnabledProperty, value);
        }

        public CustomMappingsTopRightControl()
        {
            InitializeComponent();
            // Set the DataContext of the UC to itself for binding.
            DataContext = this;
            // Load mappings and initialize commands.
            CustomTagMappings = LoadMapping();
            MoveUpCommand = new RelayCommand<CustomTagMap>(MoveMappingUp, CanMoveUp);
            MoveDownCommand = new RelayCommand<CustomTagMap>(MoveMappingDown, CanMoveDown);
        }

        // Loads mappings from XML.
        private ObservableCollection<CustomTagMap> LoadMapping()
        {
            CustomTagMapXmlService xmlService = new CustomTagMapXmlService();
            return xmlService.LoadMappings();
        }

        // Opens the NewMappingWindow to add a new mapping.
        private void btnAddMapping_Click(object sender, RoutedEventArgs e)
        {
            NewMappingWindow newMappingWindow = new NewMappingWindow();
            bool? result = newMappingWindow.ShowDialog();
            if (result == true && newMappingWindow.CreatedMapping != null)
            {
                CustomTagMappings.Add(newMappingWindow.CreatedMapping);
                UpdatePriorities();
            }
        }

        // Deletes the selected mapping.
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

        // Opens the EditMappingWindow to edit the selected mapping.
        private void btnEditMapping_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is CustomTagMap mapping)
            {
                // Set the owner to the parent window.
                EditMappingWindow editWindow = new EditMappingWindow(mapping)
                {
                    Owner = Window.GetWindow(this)
                };
                bool? result = editWindow.ShowDialog();
                if (result == true)
                {
                    lvMappings.Items.Refresh();
                }
            }
        }

        // Saves all mappings to XML.
        private void btnSaveMapping_Click(object sender, RoutedEventArgs e)
        {
            CustomTagMapXmlService xmlService = new CustomTagMapXmlService();
            xmlService.SaveMappings(CustomTagMappings);
        }

        // Deletes all mappings after confirmation.
        private void btnDeleteAllMappings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to delete all mappings from disk?",
                                                      "Confirm Delete",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string filePath = "mappings.xml"; // Replace with the actual file path if needed.
                    CustomTagMapXmlService service = new CustomTagMapXmlService();
                    service.DeleteAllMappings(filePath);
                    CustomTagMappings.Clear();
                    MessageBox.Show("All mappings have been deleted.", "Deletion Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting mappings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Updates the priority property for each mapping.
        private void UpdatePriorities()
        {
            for (int i = 0; i < CustomTagMappings.Count; i++)
            {
                CustomTagMappings[i].Priority = i;
            }
        }

        // Moves the selected mapping up in the list.
        public void MoveMappingUp(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index > 0)
            {
                CustomTagMappings.Move(index, index - 1);
                CollectionViewSource.GetDefaultView(CustomTagMappings).Refresh();
            }
        }

        // Moves the selected mapping down in the list.
        public void MoveMappingDown(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index < CustomTagMappings.Count - 1)
            {
                CustomTagMappings.Move(index, index + 1);
                CollectionViewSource.GetDefaultView(CustomTagMappings).Refresh();
            }
        }

        // Determines if the mapping can be moved up.
        private bool CanMoveUp(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            return index > 0;
        }

        // Determines if the mapping can be moved down.
        private bool CanMoveDown(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            return index < CustomTagMappings.Count - 1;
        }
    }
}
