/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

using JsonFileReader;
using Services.LoraAutoSort.Classes;
using System.Security.Cryptography;

namespace Services.LoraAutoSort
{
    public class FileControllerService
    {
        public FileControllerService()
        {

        }

        public async Task ComputeFolder(IProgress<ProgressReport> progress,
                                        string sourcePath, string targetPath,
                                        bool moveInsteadOfCopy, bool overrideExistingFiles)
        {
            progress?.Report(new ProgressReport
            {
                Percentage = 0,
                StatusMessage = "Starting processing..."
            });

            var jsonReader = new JsonInfoFileReaderService(sourcePath);
            List<ModelClass> models = await jsonReader.GetModelData(sourcePath);

            progress?.Report(new ProgressReport
            {
                Percentage = 0,
                StatusMessage = $"Number of LoRa's found: {models.Count}"
            });

            if (models == null || models.Count == 0)
            {
                // Report error and stop processing.
                progress?.Report(new ProgressReport
                {
                    Percentage = 0,
                    StatusMessage = "No Models in selected folders",
                    IsSuccessful = false
                });
                return;
            }

            var fileCopyService = new FileCopyService();
            // ProcessModelClasses now reports progress and uses our new ProgressReport type.
            fileCopyService.ProcessModelClasses(progress, models, sourcePath, targetPath, moveInsteadOfCopy, overrideExistingFiles).ToList();

            progress?.Report(new ProgressReport
            {
                Percentage = 100,
                StatusMessage = "Finished processing.",
                IsSuccessful = true
            });
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
        public string ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
