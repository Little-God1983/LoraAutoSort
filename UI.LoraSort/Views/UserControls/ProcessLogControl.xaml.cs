using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using UI.LoraSort.ViewModels;

namespace UI.LoraSort.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ProcessLogControl.xaml
    /// </summary>
    public partial class ProcessLogControl : UserControl
    {
        public ProcessLogControl()
        {
            InitializeComponent();
            this.Loaded += ProcessLogControl_Loaded;
        }

        private void ProcessLogControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
            }
        }

        private void LogEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && logListView.Items.Count > 0)
            {
                logListView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    logListView.ScrollIntoView(logListView.Items[logListView.Items.Count - 1]);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
