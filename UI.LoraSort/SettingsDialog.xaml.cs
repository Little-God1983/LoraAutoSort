using System.Windows;

namespace UI.LoraSort
{
    public partial class SettingsDialog : Window
    {
        public string ApiKey
        {
            get => SettingsUC.ApiKey;
            set => SettingsUC.ApiKey = value;
        }

        public SettingsDialog()
        {
            InitializeComponent();
            SettingsUC.SaveClicked += (s, e) => { this.DialogResult = true; };
            SettingsUC.CancelClicked += (s, e) => { this.DialogResult = false; };
        }

        protected override void OnContentRendered(System.EventArgs e)
        {
            base.OnContentRendered(e);
            // Load and decrypt API key on dialog open
            ApiKey = SettingsManager.LoadApiKey();
        }
    }
}
