﻿/*
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
            return fileCopyServicere.ProcessModelClasses(models, sourcePath, targetPath, moveInsteadOfCopy, overrideExistingFiles).ToList();
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
