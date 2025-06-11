using System.Net.Http;
using System.Threading.Tasks;

namespace Services.LoraAutoSort.Services
{
    /// <summary>
    /// Default implementation for communicating with the Civitai API.
    /// </summary>
    public class CivitaiApiClient : ICivitaiApiClient
    {
        private readonly HttpClient _httpClient;

        public CivitaiApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://civitai.com/api/v1/");
            }
        }

        public async Task<string> GetModelVersionByHashAsync(string sha256Hash)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"model-versions/by-hash/{sha256Hash}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetModelAsync(string modelId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"models/{modelId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}

