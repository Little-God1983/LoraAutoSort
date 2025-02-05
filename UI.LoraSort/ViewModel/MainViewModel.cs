using Services.LoraAutoSort.Classes;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UI.LoraSort.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<CustomTagMap> CustomTagMappings { get; set; } = new ObservableCollection<CustomTagMap>();

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        public MainViewModel()
        {
            MoveUpCommand = new RelayCommand<CustomTagMap>(MoveMappingUp);
            MoveDownCommand = new RelayCommand<CustomTagMap>(MoveMappingDown);
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
    }
}
