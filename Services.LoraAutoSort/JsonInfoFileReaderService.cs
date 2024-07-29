using Services.LoraAutoSort.Classes;
using System.Text.Json;

namespace JsonFileReader
{
    public class JsonInfoFileReaderService
    {
        private readonly string _loraInfoBasePath;

        public JsonInfoFileReaderService(string jsonFilePath)
        {
            _loraInfoBasePath = jsonFilePath;
        }
        public List<ModelClass> GetModelData(string jsonFilePath)
        {
            List<ModelClass> modelData = new List<ModelClass>();
            List<FileInfo> fileInfos = GetAllFiles();

            foreach (FileInfo fileInfo in fileInfos)
            {
                JsonDocument jdoc = LoadJsonDocument(fileInfo.FullName);
                modelData.Add(new ModelClass() 
                { fileInfo = fileInfo,
                    DiffusionBaseModel = GetBaseModelName(jdoc), 
                    CivitaiCategory = GetFirstMatchingCategory(jdoc.RootElement) 
                });
            }
            return  modelData;
        }

        private List<FileInfo> GetAllFiles()
        {
            List<FileInfo> files = new List<FileInfo>();
            try
            {
                // Ensure the path is a directory and not a file path
                if (Directory.Exists(_loraInfoBasePath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(_loraInfoBasePath);
                    foreach (var file in dirInfo.GetFiles("*.info"))
                    {
                        files.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return files;
        }

        private JsonDocument LoadJsonDocument(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            // The using statement is removed to allow the document to be used outside of this method
            return JsonDocument.Parse(jsonData);
        }

        private string GetBaseModelName(JsonDocument doc)
        {
            JsonElement root = doc.RootElement;
            return root.GetProperty("baseModel").GetString();
        }

        private CivitaiBaseCategories GetFirstMatchingCategory(JsonElement root)
        {
            if (root.TryGetProperty("model", out JsonElement model) && model.TryGetProperty("tags", out JsonElement tagsElement))
            {
                foreach (JsonElement tagElement in tagsElement.EnumerateArray())
                {
                    string tag = tagElement.GetString();
                    if (Enum.TryParse<CivitaiBaseCategories>(tag.Replace(" ", "_").ToUpper(), out CivitaiBaseCategories category))
                    {
                        return category; // Return the first match
                    }
                }
            }
            return CivitaiBaseCategories.Unknown; // Return Unknown if no match is found
        }

    }
}