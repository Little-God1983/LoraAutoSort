using JsonFileReader;
using Services.LoraAutoSort.Classes;

namespace Services.LoraAutoSort
{
    public class ControllerService
    {
        public ControllerService()
        {

        }

        public List<OperationResult> ComputeFolder(string sourcePath, string targetPath, bool moveInsteadOfCopy, bool overrideExistingFiles)
        {
            JsonInfoFileReaderService jsonInfoFileReaderService = new JsonInfoFileReaderService(sourcePath);
            List<ModelClass> models = jsonInfoFileReaderService.GetModelData(sourcePath);

            if (models == null || models.Count == 0)
            {
                return new List<OperationResult>() { new OperationResult()
                {
                    IsSuccessful = false, Message = "No Models in selected folders" }
                };
            }
            FileCopyService fileCopyServicere = new FileCopyService();
            //return fileCopyServicere.ProcessModelClasses(models, sourcePath, targetPath, moveInsteadOfCopy, overrideExistingFiles).ToList();
            return null;
        }

        public bool EnoughFreeSpaceOnDisk(string sourcePath, string targetPath)
        {
            long folderSize = GetDirectorySize(sourcePath);
            long availableSpace = GetAvailableSpace(targetPath);

            return folderSize <= availableSpace;
        }
        // Method to get the size of a directory
        public static long GetDirectorySize(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory '{folderPath}' does not exist.");
            }

            long size = 0;

            // Get the size of files in the directory and its subdirectories
            foreach (string file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                size += fileInfo.Length;
            }

            return size;
        }

        // Method to get the available space on the drive
        public static long GetAvailableSpace(string folderPath)
        {
            DriveInfo drive = new DriveInfo(Path.GetPathRoot(folderPath));
            return drive.AvailableFreeSpace;
        }

    }
}
