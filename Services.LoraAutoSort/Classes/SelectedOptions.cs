namespace Services.LoraAutoSort.Classes
{
    public class SelectedOptions
    {
        public string BasePath { get; set; }
        public string TargetPath { get; set; }
        public bool IsMoveOperation { get; set; }
        public bool OverrideFiles { get; set; }
        public bool CreateBaseFolders { get; set; }
        public bool UseCustomMappings { get; set; }
    }
}
