using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using Serilog;

namespace Services.LoraAutoSort.Services
{
    public class CivitaiMetaDataService
    {
        /// <summary>
        /// Extracts JSON metadata from a safetensors file, retrieves the Civitai URL (from "__metadata__" or "modelUrl"),
        /// extracts the model id from that URL, constructs the API endpoint URL, and calls the Civitai API to retrieve
        /// full model information.
        /// </summary>
        /// <param name="safetensorsFilePath">Full path to the safetensors file.</param>
        /// <param name="apiKey">
        /// (Optional) Your Civitai API key; if provided, it will be used in the Authorization header.
        /// </param>
        /// <returns>A JSON string containing the model information from the Civitai API.</returns>
        public async Task<string> GetModelVersionInformationFromCivitaiAsync(string sha256Hash)
        {

            // Step 4: Build the API endpoint URL.
            string apiUrl = $"https://civitai.com/api/v1/model-versions/by-hash/{sha256Hash}";
            Log.Debug("Constructed API URL: " + apiUrl);

            // Step 5: Call the Civitai API.
            using (HttpClient client = new HttpClient())
            {

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error("API call failed with status code: " + response.StatusCode + $"for SHA256Hash {sha256Hash}");
                    throw new Exception("API call failed with status code: " + response.StatusCode);
                }

                string apiResponse = await response.Content.ReadAsStringAsync();
                return apiResponse;
            }
        }
        public async Task<string> GetModelInformationAsync(string safetensorsFilePath, string apiKey = null)
        {
            // Step 1: Extract the JSON metadata from the safetensors file.
            string metadataJson = ExtractMetadata(safetensorsFilePath);
            Console.WriteLine("Extracted Metadata JSON:");
            Console.WriteLine(metadataJson);
            Console.WriteLine();

            // Step 2: Parse the JSON metadata using System.Text.Json.
            using (JsonDocument doc = JsonDocument.Parse(metadataJson))
            {
                JsonElement root = doc.RootElement;
                string civitaiUrl = null;

                // Try to retrieve the URL from "__metadata__"->"civitai"
                if (root.TryGetProperty("__metadata__", out JsonElement metaElement))
                {
                    if (metaElement.TryGetProperty("civitai", out JsonElement civitaiElement))
                    {
                        civitaiUrl = civitaiElement.GetString();
                    }
                }

                // Fallback: Try the "modelUrl" property.
                if (string.IsNullOrEmpty(civitaiUrl) && root.TryGetProperty("modelUrl", out JsonElement modelUrlElement))
                {
                    civitaiUrl = modelUrlElement.GetString();
                }

                if (string.IsNullOrEmpty(civitaiUrl))
                {
                    throw new Exception("No Civitai URL found in metadata.");
                }
                Console.WriteLine("Found Civitai URL: " + civitaiUrl);

                // Step 3: Extract the model id using a regular expression.
                // Expected URL format: https://civitai.com/models/1234
                Regex regex = new Regex(@"civitai\.com/models/(\d+)", RegexOptions.IgnoreCase);
                Match match = regex.Match(civitaiUrl);
                if (!match.Success)
                {
                    throw new Exception("Failed to extract model id from the Civitai URL.");
                }
                string modelId = match.Groups[1].Value;
                Console.WriteLine("Extracted Model ID: " + modelId);

                // Step 4: Build the API endpoint URL.
                string apiUrl = $"https://civitai.com/api/v1/models/{modelId}";
                Console.WriteLine("Constructed API URL: " + apiUrl);
                Console.WriteLine();

                // Step 5: Call the Civitai API.
                using (HttpClient client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                    }

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("API call failed with status code: " + response.StatusCode);
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Civitai API Response:");
                    Console.WriteLine(apiResponse);
                    return apiResponse;
                }
            }
        }

        /// <summary>
        /// Extracts the JSON metadata from a safetensors file.
        /// The file format begins with an 8-byte little-endian unsigned integer indicating the length of the JSON header,
        /// followed immediately by the JSON header.
        /// </summary>
        /// <param name="filePath">Path to the safetensors file.</param>
        /// <returns>A UTF-8 encoded string containing the JSON metadata.</returns>
        private string ExtractMetadata(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Read the first 8 bytes (header length)
                byte[] lengthBytes = new byte[8];
                int bytesRead = fs.Read(lengthBytes, 0, 8);
                if (bytesRead < 8)
                {
                    throw new Exception("File is too short to contain a valid header length.");
                }

                // Convert the first 8 bytes into a UInt64 (little-endian)
                ulong headerLength = BitConverter.ToUInt64(lengthBytes, 0);

                // Read the JSON header bytes.
                byte[] headerBytes = new byte[headerLength];
                bytesRead = fs.Read(headerBytes, 0, (int)headerLength);
                if ((ulong)bytesRead < headerLength)
                {
                    throw new Exception("File is too short to contain the full header.");
                }

                // Convert the header bytes into a UTF8 string.
                return Encoding.UTF8.GetString(headerBytes);
            }
        }

        internal string GetBaseModelName(string modelInfoApiResponse)
        {
            using JsonDocument doc = JsonDocument.Parse(modelInfoApiResponse);
            return doc.RootElement.GetProperty("baseModel").ToString();
        }

        internal string GetModelId(string modelInfoApiResponse)
        {
            using JsonDocument doc = JsonDocument.Parse(modelInfoApiResponse);
            return doc.RootElement.GetProperty("modelId").ToString();
        }

        internal async Task<string> GetModelInformationFromCivitaiAsync(string modelId)
        {
            // Step 4: Build the API endpoint URL.
            string apiUrl = $"https://civitai.com/api/v1/models/{modelId}";
            Log.Debug("Constructed API URL: " + apiUrl);

            // Step 5: Call the Civitai API.
            using (HttpClient client = new HttpClient())
            {

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error("API call failed with status code: " + response.StatusCode + $"for ModelId {modelId}");
                    throw new Exception("API call failed with status code: " + response.StatusCode);
                }

                string apiResponse = await response.Content.ReadAsStringAsync();
                return apiResponse;
            }
        }

        internal List<string> GetTagsFromModelInfo(string modelInfoApiResponse)
        {
            var tags = new List<string>();

            using (JsonDocument doc = JsonDocument.Parse(modelInfoApiResponse))
            {
                JsonElement root = doc.RootElement;

                // Check if 'tags' property exists and is an array
                if (root.TryGetProperty("tags", out JsonElement tagsElement) &&
                    tagsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement tagValue in tagsElement.EnumerateArray())
                    {
                        string tagString = tagValue.GetString();
                        if (!string.IsNullOrWhiteSpace(tagString))
                        {
                            tags.Add(tagString);
                        }
                    }
                }
            }

            return tags;
        }

        internal DiffusionTypes GetModelType(string modelInfoApiResponse)
        {
            using (JsonDocument doc = JsonDocument.Parse(modelInfoApiResponse))
            {
                JsonElement root = doc.RootElement;

                // 1. Check if `type` is directly in the root
                if (root.TryGetProperty("type", out JsonElement typeElement) &&
                    typeElement.ValueKind == JsonValueKind.String)
                {
                    string typeString = typeElement.GetString()?.Replace(" ", "").ToUpper();

                    if (Enum.TryParse(typeString, true, out DiffusionTypes modelType))
                    {
                        return modelType;
                    }
                }

                // 2. Check if `type` is inside `model`
                if (root.TryGetProperty("model", out JsonElement modelElement) &&
                    modelElement.TryGetProperty("type", out JsonElement nestedTypeElement) &&
                    nestedTypeElement.ValueKind == JsonValueKind.String)
                {
                    string nestedTypeString = nestedTypeElement.GetString()?.Replace(" ", "").ToUpper();

                    if (Enum.TryParse(nestedTypeString, true, out DiffusionTypes nestedModelType))
                    {
                        return nestedModelType;
                    }
                }
            }
            return DiffusionTypes.OTHER; // Default if no match is found
        }


    }
}