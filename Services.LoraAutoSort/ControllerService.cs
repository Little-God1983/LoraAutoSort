using JsonFileReader;
using Services.LoraAutoSort.Classes;

namespace Services.LoraAutoSort
{
    public class ControllerService
    {
        public ControllerService()
        {

        }

        public List<OperationResult> ComputeFolder(string sourcePath, string targetPath, bool moveInsteadofDelete)
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
            return fileCopyServicere.ProcessModelClasses(models, sourcePath, targetPath).ToList();
        }
    }
}
