using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using Services.LoraAutoSort.Classes;
using Services.LoraAutoSort.Services;
using UI.LoraSort.Infrastructure;

namespace UI.LoraSort.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Existing custom mapping properties
        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; } = new ObservableCollection<CustomTagMap>();
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand CopyErrorLogsCommand { get; }
        public ICommand ExportErrorLogsCommand { get; }



        // New logging properties
        public ObservableCollection<ProgressReport> LogEntries { get; } = new ObservableCollection<ProgressReport>();
        public ICollectionView LogEntriesView { get; }

        private bool _showErrorsOnly = true;
        public bool ShowErrorsOnly
        {
            get => _showErrorsOnly;
            set
            {
                if (_showErrorsOnly != value)
                {
                    _showErrorsOnly = value;
                    OnPropertyChanged(nameof(ShowErrorsOnly));
                    LogEntriesView.Refresh();
                }
            }
        }

        private string _buildNumber;
        public string BuildNumber
        {
            get => _buildNumber;
            set
            {
                _buildNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildNumber)));
            }
        }

        public MainViewModel()
        {
            CustomTagMappings = new CustomTagMapXmlService().LoadMappings();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            BuildNumber = $"Build {version}";

            MoveUpCommand = new RelayCommand<CustomTagMap>(MoveMappingUp);
            MoveDownCommand = new RelayCommand<CustomTagMap>(MoveMappingDown);

            CopyErrorLogsCommand = new RelayCommand(() =>
            {
                var errorMessages = LogEntries
                    .Where(e => e.IsSuccessful.HasValue && !e.IsSuccessful.Value)
                    .Select(e => e.StatusMessage);
                string textToCopy = string.Join(Environment.NewLine, errorMessages);
                Clipboard.SetText(textToCopy);
            });


            ExportErrorLogsCommand = new RelayCommand(() =>
            {
                // Filter error messages
                var errorMessages = LogEntries
                    .Where(e => e.IsSuccessful.HasValue && !e.IsSuccessful.Value)
                    .Select(e => e.StatusMessage);
                string textToExport = string.Join(Environment.NewLine, errorMessages);

                // Let the user choose where to save the file using a SaveFileDialog.
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Text File (*.txt)|*.txt",
                    Title = "Export Error Logs"
                };

                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllText(sfd.FileName, textToExport);
                }
            });

            // Set up the filtered view for logs.
            LogEntriesView = CollectionViewSource.GetDefaultView(LogEntries);
            LogEntriesView.Filter = o =>
            {
                if (o is ProgressReport report)
                {
                    if (ShowErrorsOnly)
                    {
                        // Only show errors (IsSuccessful explicitly false)
                        return report.IsSuccessful.HasValue && !report.IsSuccessful.Value;
                    }
                    return true;
                }
                return false;
            };
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void ClearLogs()
        {
            LogEntries.Clear();
        }

        public void AppendLog(string message, bool isError)
        {
            LogEntries.Add(new ProgressReport
            {
                StatusMessage = message,
                // Assuming that an error should be indicated by IsSuccessful == false.
                IsSuccessful = !isError
            });
        }
    }
}
