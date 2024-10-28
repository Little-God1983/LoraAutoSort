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
            List<OperationResult> results = new List<OperationResult>();

            foreach (var model in models)
            {
                if(model.SkipFile)
                {
                    results.Add(new OperationResult { IsSuccessful = false, Message = $"File '{model.ModelName}' has no metaData => File is skipped." });
                    continue;
                }
                string modelDirectory = Path.Combine(targetPath, model.DiffusionBaseModel, model.CivitaiCategory.ToString());
                results.Add(EnsureFolderExists(modelDirectory));

                foreach (var modelFile in model.AssociatedFilesInfo)
                {
                    string source = modelFile.FullName;
                    string target = Path.Combine(modelDirectory, modelFile.Name);
                    try
                    {
                        CopyMove(overrideExistingFiles, source, target, moveInsteadOfCopy);
                        results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{modelFile.Name}' copied to '{modelDirectory}'." });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new OperationResult { IsSuccessful = false, Message = $"Error copying file '{modelFile.Name}': {ex.Message}" });
                    }
                }
            }
            return results;
        }

        private static void CopyMove(bool overrideExistingFiles, string source, string target, bool moveInsteadOfCopy)
        {
            if (moveInsteadOfCopy)
            {
                File.Move(source, target, overrideExistingFiles);
            }
            else
            {
                File.Copy(source, target, overrideExistingFiles);                
            }
        }

        private static string ChangeEnding(string fileName, string newEnding)
        {
            //remove double extension like civitai.info
            fileName = fileName.Split('.')[0] + newEnding;
            //destFile = destFile.Split('.')[0] + newEnding;
            return fileName;
        }
    }
}