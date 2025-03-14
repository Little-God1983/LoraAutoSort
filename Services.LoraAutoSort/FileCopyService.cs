﻿/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

using Serilog;
using Services.LoraAutoSort.Classes;

namespace JsonFileReader
{
    public class FileCopyService
    {
        public FileCopyService()
        {

        }
        private void EnsureFolderExists(IProgress<ProgressReport>? progress, string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    progress?.Report(new ProgressReport { IsSuccessful = true, StatusMessage = $"Directory '{directoryPath}' created successfully." });
                }
                else
                {
                    progress?.Report(new ProgressReport { IsSuccessful = true, StatusMessage = $"Directory '{directoryPath}' already exists." });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create directory '{directoryPath}' {ex.Message}");
                progress?.Report(new ProgressReport { IsSuccessful = false, StatusMessage = $"Failed to create directory '{directoryPath}'" });
            }
        }

        public bool ProcessModelClasses(IProgress<ProgressReport>? progress, List<ModelClass> models, string sourcePaht, string targetPath, bool moveInsteadOfCopy, bool overrideExistingFiles, CancellationToken cancellationToken, bool NoBaseModelFolders)
        {
            int totalModels = models.Count;
            int currentModel = 0;
            bool hasErrors = false;

            foreach (var model in models)
            {
                // Update progress based on model index.
                int percentage = (int)((double)currentModel / totalModels * 100);

                // Throw if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();
         
                if (model.NoMetaData)
                {
                    progress?.Report(new ProgressReport { IsSuccessful = false, Percentage = percentage, StatusMessage = $"File '{model.ModelName}' has no metaData => File is skipped." });
                    hasErrors = true;
                    continue;
                }
                else if (model.ErrorOnRetrievingMetaData)
                {
                    //No neeed to make a progress report since we allready display the failed API call
                    hasErrors = true;
                    continue;
                }

                if (model.ModelType != DiffusionTypes.LORA)
                {
                    progress?.Report(new ProgressReport { IsSuccessful = false, Percentage = percentage, StatusMessage = $"File '{model.ModelName}' is not a Lora. It is of Type: {model.ModelType.ToString()} => File is skipped." });
                    hasErrors = true;
                    continue;
                }

                string modelDirectory;
                if (NoBaseModelFolders)
                {
                    modelDirectory = Path.Combine(targetPath, model.CivitaiCategory.ToString());
                }
                else
                {
                    modelDirectory = Path.Combine(targetPath, model.DiffusionBaseModel, model.CivitaiCategory.ToString());
                }

                EnsureFolderExists(progress, modelDirectory);

                foreach (var modelFile in model.AssociatedFilesInfo)
                {
                    string source = modelFile.FullName;
                    string target = Path.Combine(modelDirectory, modelFile.Name);
                    try
                    {
                        if (moveInsteadOfCopy)
                        {
                            File.Move(source, target, overrideExistingFiles);
                            progress?.Report(new ProgressReport { IsSuccessful = true, Percentage = percentage, StatusMessage = $"File '{modelFile.Name}' moved to '{modelDirectory}'." });
                        }
                        else
                        {
                            File.Copy(source, target, overrideExistingFiles);
                            progress?.Report(new ProgressReport { IsSuccessful = true, Percentage = percentage, StatusMessage = $"File '{modelFile.Name}' copied to '{modelDirectory}'." });
                        }
                    }
                    catch (Exception ex)
                    {
                        progress?.Report(new ProgressReport { IsSuccessful = false, Percentage = percentage, StatusMessage = $"Error copying file '{modelFile.Name}' Reason: {ex.Message}" });
                        hasErrors = true;
                    }
                }
                currentModel++;
            }

            return hasErrors;
        }
    }
}