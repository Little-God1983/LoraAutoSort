using System.Threading.Tasks;

namespace Services.LoraAutoSort.Services
{
    /// <summary>
    /// Abstraction over the Civitai HTTP API. Allows mocking in tests and
    /// centralises all endpoint calls.
    /// </summary>
    public interface ICivitaiApiClient
    {
        Task<string> GetModelVersionByHashAsync(string sha256Hash);
        Task<string> GetModelAsync(string modelId);
    }
}

