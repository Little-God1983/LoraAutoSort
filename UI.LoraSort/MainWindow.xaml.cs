using System;
using System.Windows;
using UI.LoraSort.ViewModels;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Subscribe to log updates for auto-scrolling.
            if (DataContext is MainViewModel vm)
            {
                vm.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
            }
        }

        private void LogEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                && logListView.Items.Count > 0)
            {
                logListView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    logListView.ScrollIntoView(logListView.Items[logListView.Items.Count - 1]);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
