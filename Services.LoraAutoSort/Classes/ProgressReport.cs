
namespace Services.LoraAutoSort.Classes
{
    public class ProgressReport
    {
        public int Percentage { get; set; }
        public string StatusMessage { get; set; }
        // null means the update is just informational, true indicates success, false indicates an error.
        public bool? IsSuccessful { get; set; }
    }
}
