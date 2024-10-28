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
            foreach (ModelClass model in modelData)
            {
                if(model.SkipFile == true) { continue; }
                model.CivitaiCategory = GetMatchingCategory(model);
                string baseModelName = GetBaseModelName(model);
                model.DiffusionBaseModel = baseModelName == "SDXL" ? "SDXL 1.0":baseModelName;

            }
            return modelData;
        }

        private CivitaiBaseCategories GetMatchingCategory(ModelClass model)
        {
            // Try loading tags from the ".json" file.
            var uniqueTags = new HashSet<string>(LoadTagsFromFile(model, ".civitai.info","tags"));

            // If the category is still UNKNOWN, attempt to load additional tags from ".cm-info.json".
            uniqueTags.UnionWith(LoadTagsFromFile(model, ".cm-info.json", "Tags"));

            model.Tags = uniqueTags.ToList();

            // Determine and return the category based on tags.
            return GetCategoryFromTags(model.Tags);
        }

        // Helper method to load tags from a specified file ending.
        private HashSet<string> LoadTagsFromFile(ModelClass model, string ending, string searchTerm)
        {
            var file = model.AssociatedFilesInfo.FirstOrDefault(x => x.FullName.Contains(ending));
            if (file == null)
            {
                return new HashSet<string>();
            }

            JsonDocument jdoc = LoadJsonDocument(file.FullName);
            return GetTagsFromJson(jdoc.RootElement, searchTerm);
        }

        private CivitaiBaseCategories GetCategoryFromTags(List<string> tags)
        {
            //Implement CustomTag
            foreach (string tag in tags)
            {
                if (Enum.TryParse<CivitaiBaseCategories>(tag.Replace(" ", "_").ToUpper(), out CivitaiBaseCategories category))
                {
                    return category; // Return the first match
                }
            }
            return CivitaiBaseCategories.UNKNOWN;
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
                        if (StaticFileTypes.ModelExtensions.Contains(Path.GetExtension(file.FullName)))
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
                ModelClass model = new ModelClass
                {
                    ModelName = group.Key,
                    AssociatedFilesInfo = group.Value,
                    CivitaiCategory = CivitaiBaseCategories.UNKNOWN // Set your desired category here
                };

                //If <= 1 That means there is no MetaData so the file will be skipped
                if (model.AssociatedFilesInfo.Count <= 1) { model.SkipFile = true; }
                modelClasses.Add(model);
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

        private string GetBaseModelName(ModelClass model)
        {
            var fileCivitai = model.AssociatedFilesInfo.FirstOrDefault(x => x.FullName.Contains(".civitai.info"));
            var fileJson = model.AssociatedFilesInfo.FirstOrDefault(x => x.Extension == ".json");
            if (fileCivitai != null)
            {
                JsonDocument jdoc = LoadJsonDocument(fileCivitai.FullName);
                JsonElement root = jdoc.RootElement;
                return root.GetProperty("baseModel").GetString();
            }else if(fileJson != null)
            {
                JsonDocument jdoc = LoadJsonDocument(fileJson.FullName);
                JsonElement root = jdoc.RootElement;
                if (root.TryGetProperty("sd version", out JsonElement tagElement) && tagElement.ValueKind == JsonValueKind.String)
                {
                    return tagElement.GetString();
                }
            }
            
            return "UNKNOWN";
        }

        private HashSet<string> GetTagsFromJson(JsonElement root,string searchTerm)
        {
            HashSet<string> tags = new HashSet<string>();

            if (root.TryGetProperty("model", out JsonElement model) && model.TryGetProperty(searchTerm, out JsonElement tagsElement))
            {
                GetTagsFromJsonElement(tags, tagsElement);
            }
            else if (root.TryGetProperty(searchTerm, out JsonElement tagElement))
            {
                GetTagsFromJsonElement(tags, tagElement);
            }

            return tags;
        }

        private static void GetTagsFromJsonElement(HashSet<string> tags, JsonElement tagsElement)
        {
            foreach (JsonElement tagElement in tagsElement.EnumerateArray())
            {
                string tag = tagElement.GetString();
                if (!string.IsNullOrEmpty(tag))
                {
                    tags.Add(tag);
                }
            }
        }
    }
}