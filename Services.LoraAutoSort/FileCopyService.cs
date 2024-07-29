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

        //public IEnumerable<OperationResult> CopyToBaseModelFolder(string[] filesToCopy)
        //{
        //    List<OperationResult> results = new List<OperationResult>();
        //    results.Add(EnsureFolderExists());

        //    foreach (string file in filesToCopy)
        //    {
        //        string destFile = Path.Combine(_baseModelFolder, Path.GetFileName(file));
        //        try
        //        {
        //            if (File.Exists(file))
        //            {
        //                File.Copy(file, destFile, true);
        //                results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{file}' copied to '{_baseModelFolder}'." });
        //            }
        //            else
        //            {
        //                results.Add(new OperationResult { IsSuccessful = false, Message = $"File '{file}' not found, skipping copy." });
        //            }
        //        }
        //        catch (IOException ioEx)
        //        {
        //            results.Add(new OperationResult { IsSuccessful = false, Message = $"I/O Error copying file '{file}': {ioEx.Message}" });
        //        }
        //        catch (UnauthorizedAccessException uaEx)
        //        {
        //            results.Add(new OperationResult { IsSuccessful = false, Message = $"Access violation copying file '{file}': {uaEx.Message}" });
        //        }
        //        catch (Exception ex)
        //        {
        //            results.Add(new OperationResult { IsSuccessful = false, Message = $"An error occurred copying file '{file}': {ex.Message}" });
        //        }
        //    }
        //    return results;
        //}


        //public IEnumerable<OperationResult> ProcessModelClasses(List<ModelClass> models, string targetPath)
        //{
        //    List<OperationResult> results = new List<OperationResult>();

        //    foreach (var model in models)
        //    {
        //        string modelDirectory = Path.Combine(targetPath, model.BaseModel, model.CivitaiCategory.ToString());
        //        results.Add(EnsureFolderExists(modelDirectory));

        //        if (model.fileInfo != null && model.fileInfo.Exists)
        //        {
        //            string destFile = Path.Combine(targetPath, model.fileInfo.Name);
        //            try
        //            {
        //                // string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(model.fileInfo.FullName);

        //                File.Copy(model.fileInfo.FullName, Path.Combine(targetPath, model.fileInfo.Name), true);

        //                string fileName = ChangeEnding(model.fileInfo.Name, ".safetensors");
        //                File.Copy(Path.Combine(model.fileInfo.DirectoryName, fileName), Path.Combine(targetPath, fileName), true);
        //                results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{model.fileInfo.DirectoryName + fileName}' copied to '{modelDirectory}'." });

        //                fileName = ChangeEnding(model.fileInfo.Name, ".json");
        //                File.Copy(Path.Combine(model.fileInfo.DirectoryName, fileName), Path.Combine(targetPath, fileName), true);
        //                results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{model.fileInfo.Name}' copied to '{modelDirectory}'." });

        //                fileName = ChangeEnding(model.fileInfo.Name, ".preview.png");
        //                File.Copy(Path.Combine(model.fileInfo.DirectoryName, fileName), Path.Combine(targetPath, fileName), true);
        //                results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{model.fileInfo.Name}' copied to '{modelDirectory}'." });
        //            }
        //            catch (Exception ex)
        //            {
        //                results.Add(new OperationResult { IsSuccessful = false, Message = $"Error copying file '{model.fileInfo.Name}': {ex.Message}" });
        //            }
        //        }
        //        else
        //        {
        //            results.Add(new OperationResult { IsSuccessful = false, Message = "File info is null or file does not exist, skipping copy." });
        //        }
        //    }

        //    return results;
        //}

        public IEnumerable<OperationResult> ProcessModelClasses(List<ModelClass> models, string targetPath)
        {
            List<OperationResult> results = new List<OperationResult>();

            foreach (var model in models)
            {
                string modelDirectory = Path.Combine(targetPath, model.BaseModel, model.CivitaiCategory.ToString());
                results.Add(EnsureFolderExists(modelDirectory));

                if (model.fileInfo != null && model.fileInfo.Exists)
                {
                    try
                    {
                        string destFile = Path.Combine(modelDirectory, model.fileInfo.Name);
                        File.Copy(model.fileInfo.FullName, destFile, true);

                        string fileName = ChangeEnding(model.fileInfo.Name, ".safetensors");
                        File.Copy(Path.Combine(model.fileInfo.DirectoryName, fileName), Path.Combine(modelDirectory, fileName), true);
                        results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{fileName}' copied to '{modelDirectory}'." });

                        fileName = ChangeEnding(model.fileInfo.Name, ".json");
                        File.Copy(Path.Combine(model.fileInfo.DirectoryName, fileName), Path.Combine(modelDirectory, fileName), true);
                        results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{fileName}' copied to '{modelDirectory}'." });

                        fileName = ChangeEnding(model.fileInfo.Name, ".preview.png");
                        File.Copy(Path.Combine(model.fileInfo.DirectoryName, fileName), Path.Combine(modelDirectory, fileName), true);
                        results.Add(new OperationResult { IsSuccessful = true, Message = $"File '{fileName}' copied to '{modelDirectory}'." });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new OperationResult { IsSuccessful = false, Message = $"Error copying file '{model.fileInfo.Name}': {ex.Message}" });
                    }
                }
                else
                {
                    results.Add(new OperationResult { IsSuccessful = false, Message = "File info is null or file does not exist, skipping copy." });
                }
            }

            return results;
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