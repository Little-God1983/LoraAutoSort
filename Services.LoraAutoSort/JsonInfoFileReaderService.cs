using Services.LoraAutoSort.Classes;
using Services.LoraAutoSort.Helper;
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
            modelData = GroupFilesByPrefix(jsonFilePath);
            List<FileInfo> fileInfos = GetAllFiles();

            foreach (FileInfo fileInfo in fileInfos)
            {

                //JsonDocument jdoc = LoadJsonDocument(fileInfo.FullName);
                modelData = new List<ModelClass>();


                //fileInfo = fileInfo,
                //    DiffusionBaseModel = GetBaseModelName(jdoc), 
                //    CivitaiCategory = GetFirstMatchingCategory(jdoc.RootElement)
            }
            return modelData;
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
                    //foreach (var file in dirInfo.GetFiles("*.info", SearchOption.AllDirectories))
                    //{
                    //    files.Add(file);
                    //}
                    foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (StaticFileTypes.FileEndings.Contains(Path.GetExtension(file.FullName)))
                        {
                            files.Add(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return files;
        }

        //private List<FileInfo> GetAllFiles2()
        //{
        //    List<FileInfo> files = new List<FileInfo>();
        //    try
        //    {
        //        if (Directory.Exists(_loraInfoBasePath))
        //        {
        //            string prefix = Path.Combine(_loraInfoBasePath, "Sugar_Thrillz_Fool_For_You_SDXL");

        //            // Use GetFiles with a search pattern that starts with the prefix
        //            files.AddRange(Directory.GetFiles(prefix, "*.info"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    return files;
        //}


        public static List<ModelClass> GroupFilesByPrefix(string rootDirectory)
        {
            var fileGroups = new Dictionary<string, List<FileInfo>>();

            // Get all files from the directory and subdirectories
            string[] files = Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var fileInfo = new FileInfo(filePath);
                var prefix = GetPrefix(fileInfo.Name).ToLower();

                if (!fileGroups.ContainsKey(prefix))
                {
                    fileGroups[prefix] = new List<FileInfo>();
                }
                fileGroups[prefix].Add(fileInfo);
            }

            // Create ModelClass instances from grouped files
            var modelClasses = new List<ModelClass>();

            foreach (var group in fileGroups)
            {
                modelClasses.Add(new ModelClass
                {
                    DiffusionBaseModel = group.Key,
                    fileInfo = group.Value,
                    CivitaiCategory = CivitaiBaseCategories.UNKNOWN // Set your desired category here
                });
            }

            return modelClasses;
        }

        private static string GetPrefix(string fileName)
        {
            // Extract the prefix from the file name (everything before the last underscore)
            //var lastUnderscoreIndex = fileName.LastIndexOf('_');
            //return lastUnderscoreIndex > 0 ? fileName.Substring(0, lastUnderscoreIndex) : fileName;
            return fileName.Split('.').First();
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
            return CivitaiBaseCategories.UNKNOWN; // Return Unknown if no match is found
        }

    }
}