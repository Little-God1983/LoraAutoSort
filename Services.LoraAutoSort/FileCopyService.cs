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

        string[] extensions = new string[6] { ".safetensors", ".json", ".preview.png",".preview.jpeg", ".pt", ".cm-info.json" };

        public IEnumerable<OperationResult> ProcessModelClasses(List<ModelClass> models, string sourcePaht, string targetPath, bool moveInsteadOfCopy, bool overrideExistingFiles)
        {
            List<OperationResult> results = new List<OperationResult>();

            foreach (var model in models)
            {
                string modelDirectory = Path.Combine(targetPath, model.DiffusionBaseModel, model.CivitaiCategory.ToString());
                results.Add(EnsureFolderExists(modelDirectory));

                string source = model.fileInfo.FullName;
                string target = Path.Combine(modelDirectory, model.fileInfo.Name);
                CopyMove(overrideExistingFiles, source, target, moveInsteadOfCopy);

                foreach (var extension in extensions)
                {
                    string fileName = ChangeEnding(model.fileInfo.Name, extension);
                    source = Path.Combine(model.fileInfo.DirectoryName, fileName);
                    target = Path.Combine(modelDirectory, fileName);

                    try
                    {
                        CopyMove(overrideExistingFiles, source, target, moveInsteadOfCopy);
                        results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{fileName}' copied to '{modelDirectory}'." });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new OperationResult { IsSuccessful = false, Message = $"Error copying file '{model.fileInfo.Name}': {ex.Message}" });
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