/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */
﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using UI.LoraSort.ViewModels;
using Serilog;
using Services.LoraAutoSort.Classes;
using Services.LoraAutoSort.Services;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        public MainWindow()
        {
            InitializeComponent();

            // Load custom tag mappings and initialize commands.
            CustomTagMappings = LoadMapping();
            MoveUpCommand = new RelayCommand<CustomTagMap>(MoveMappingUp, CanMoveUp);
            MoveDownCommand = new RelayCommand<CustomTagMap>(MoveMappingDown, CanMoveDown);

            // Subscribe to log updates for auto-scrolling.
            if (DataContext is MainViewModel vm)
            {
                vm.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
            }
        }

        private void LogEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && logListView.Items.Count > 0)
            {
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

        public void MoveMappingUp(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index > 0)
            {
                CustomTagMappings.Move(index, index - 1);
                CollectionViewSource.GetDefaultView(CustomTagMappings).Refresh();
            }
        }

        public void MoveMappingDown(CustomTagMap map)
        {
            int index = CustomTagMappings.IndexOf(map);
            if (index < CustomTagMappings.Count - 1)
            {
                CustomTagMappings.Move(index, index + 1);
                CollectionViewSource.GetDefaultView(CustomTagMappings).Refresh();
            }
        }

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

        private void btnEditMapping_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is CustomTagMap mapping)
            {
                EditMappingWindow editWindow = new EditMappingWindow(mapping)
                {
                    Owner = this
                };
                bool? result = editWindow.ShowDialog();
                if (result == true)
                {
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
            MessageBoxResult result = MessageBox.Show("Do you really want to delete all mappings from disk?",
                                                      "Confirm Delete",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string filePath = "mappings.xml"; // Replace with your actual file path if necessary.
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
    }
}
