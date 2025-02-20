using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Services.LoraAutoSort.Classes;

namespace UI.LoraSort.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Existing custom mapping properties
        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; } = new ObservableCollection<CustomTagMap>();
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        // New logging properties
        public ObservableCollection<ProgressReport> LogEntries { get; } = new ObservableCollection<ProgressReport>();
        public ICollectionView LogEntriesView { get; }

        private bool _showErrorsOnly;
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

        public MainViewModel()
        {
            MoveUpCommand = new RelayCommand<CustomTagMap>(MoveMappingUp);
            MoveDownCommand = new RelayCommand<CustomTagMap>(MoveMappingDown);

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
