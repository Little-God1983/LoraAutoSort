using System.Windows;
using System.Windows.Controls;

namespace UI.LoraSort.UserControls
{
    public partial class SettingsControl : UserControl
    {
        public string ApiKey
        {
            get => ApiKeyTextBox.Text;
            set => ApiKeyTextBox.Text = value;
        }

        public event RoutedEventHandler SaveClicked;
        public event RoutedEventHandler CancelClicked;

        public SettingsControl()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveClicked?.Invoke(this, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, e);
        }
    }
}
