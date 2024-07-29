using JsonFileReader;
using Services.LoraAutoSort.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LoraAutoSort
{
    public class ControllerService
    {
        public ControllerService()
        {

        }

        public List<OperationResult> ComputeFolder(string sourcePath, string targetPath)
        {
            JsonInfoFileReaderService jsonInfoFileReaderService = new JsonInfoFileReaderService(sourcePath);
            List<ModelClass> models = jsonInfoFileReaderService.GetModelData(sourcePath);
            FileCopyService fileCopyServicere = new FileCopyService();
            fileCopyServicere.ProcessModelClasses(models, targetPath);

            return new List<OperationResult>();
        }
    }
}
