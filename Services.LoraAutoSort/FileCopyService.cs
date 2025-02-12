/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

using Services.LoraAutoSort.Classes;

namespace JsonFileReader
{
    public class FileCopyService
    {
        public FileCopyService()
        {

        }
        private OperationResult EnsureFolderExists(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    return new OperationResult { IsSuccessful = true, Message = $"Directory '{directoryPath}' created successfully." };
                }
                else
                {
                    return new OperationResult { IsSuccessful = true, Message = $"Directory '{directoryPath}' already exists." };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { IsSuccessful = false, Message = $"Failed to create directory '{directoryPath}': {ex.Message}" };
            }
        }

        public IEnumerable<OperationResult> ProcessModelClasses(List<ModelClass> models, string sourcePaht, string targetPath, bool moveInsteadOfCopy, bool overrideExistingFiles)
        {
            List<OperationResult> operationResults = new List<OperationResult>();

            foreach (var model in models)
            {
                if (model.NoMetaData)
                {
                    operationResults.Add(new OperationResult { IsSuccessful = false, Message = $"File '{model.ModelName}' has no metaData => File is skipped." });
                    continue;
                }
                string modelDirectory = Path.Combine(targetPath, model.DiffusionBaseModel, model.CivitaiCategory.ToString());
                operationResults.Add(EnsureFolderExists(modelDirectory));

                foreach (var modelFile in model.AssociatedFilesInfo)
                {
                    string source = modelFile.FullName;
                    string target = Path.Combine(modelDirectory, modelFile.Name);
                    try
                    {
                        if (moveInsteadOfCopy)
                        {
                            File.Move(source, target, overrideExistingFiles);
                            operationResults.Add(new OperationResult { IsSuccessful = true, Message = $"File '{modelFile.Name}' moved to '{modelDirectory}'." });
                        }
                        else
                        {
                            File.Copy(source, target, overrideExistingFiles);
                            operationResults.Add(new OperationResult { IsSuccessful = true, Message = $"File '{modelFile.Name}' copied to '{modelDirectory}'." });
                        }
                    }
                    catch (Exception ex)
                    {
                        operationResults.Add(new OperationResult { IsSuccessful = false, Message = $"Error copying file '{modelFile.Name}': {ex.Message}" });
                    }
                }
            }
            return operationResults;
        }
    }
}