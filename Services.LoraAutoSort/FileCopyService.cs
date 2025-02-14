/*
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
                Log.Error($"Failed to create directory '{directoryPath}' { ex.Message}");
                progress?.Report(new ProgressReport { IsSuccessful = false, StatusMessage = $"Failed to create directory '{directoryPath}'" });
            }
        }

        public bool ProcessModelClasses(IProgress<ProgressReport>? progress, List<ModelClass> models, string sourcePaht, string targetPath, bool moveInsteadOfCopy, bool overrideExistingFiles)
        {
           

            //progressbar ranges from 0 - 100: we need to know what value 1% is.
            int stepVAlue = models.Count / 100;
            int total = 0;
            bool hasErrors = false;
            progress?.Report(new ProgressReport { StatusMessage = $"Number of LoRa's found: {models.Count}" });
            foreach (var model in models)
            {
                int percentage = total / stepVAlue;
                

                if (model.NoMetaData)
                {
                    progress?.Report(new ProgressReport { IsSuccessful = false, Percentage = percentage, StatusMessage = $"File '{model.ModelName}' has no metaData => File is skipped." });
                    hasErrors = true;
                    continue;
                }
                else if (model.ErrorOnRetrievingMetaData)
                {
                    progress?.Report(new ProgressReport { IsSuccessful = false, Percentage = percentage, StatusMessage = $"File '{model.ModelName}' Error on retrieving Meta Data. Check application log for infos" });
                    hasErrors = true;
                    continue;
                }

                string modelDirectory = Path.Combine(targetPath, model.DiffusionBaseModel, model.CivitaiCategory.ToString());
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
                        progress?.Report(new ProgressReport { IsSuccessful = true, Percentage = percentage, StatusMessage = $"Error copying file '{modelFile.Name}': {ex.Message}" });
                    }
                }
                total++;
            }

            return hasErrors;
        }
    }
}