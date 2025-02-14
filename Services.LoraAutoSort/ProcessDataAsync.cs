using Services.LoraAutoSort.Classes;

namespace Services.LoraAutoSort
{
    internal class ProcessData
    {
        public async Task ProcessDataAsync(IProgress<ProgressReport> progress)
        {
            progress?.Report(new ProgressReport { Percentage = 0, StatusMessage = "Starting processing..." });

            // Simulate some work with periodic progress updates.
            for (int i = 1; i <= 100; i++)
            {
                await Task.Delay(50);  // Simulate work
                progress?.Report(new ProgressReport
                {
                    Percentage = i,
                    StatusMessage = $"Processing {i}%"
                });
            }

            progress?.Report(new ProgressReport { Percentage = 100, StatusMessage = "Finished processing." });
        }
    }
}
