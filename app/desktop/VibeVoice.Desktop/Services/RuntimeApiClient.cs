using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using VibeVoice.Desktop.Models;

namespace VibeVoice.Desktop.Services;

public interface IRuntimeApiClient
{
    Task<HealthResponse> GetHealthAsync(CancellationToken cancellationToken = default);
    Task<ModelActionResponse> LoadModelAsync(LoadModelRequest request, CancellationToken cancellationToken = default);
    Task<VoiceListResponse> GetVoicesAsync(CancellationToken cancellationToken = default);
    Task<CreateJobResponse> CreateJobAsync(GenerateJobRequest request, CancellationToken cancellationToken = default);
    Task<JobResponse> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task<JobActionResponse> StopJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task<ArtifactResponse> GetArtifactAsync(string jobId, CancellationToken cancellationToken = default);
}

public sealed class RuntimeApiClient : IRuntimeApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public RuntimeApiClient(string baseAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(10),
        };
    }

    public async Task<HealthResponse> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<HealthResponse>("/health", JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty health payload.");
    }

    public async Task<ModelActionResponse> LoadModelAsync(LoadModelRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/models/load", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ModelActionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty model response.");
    }

    public async Task<VoiceListResponse> GetVoicesAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<VoiceListResponse>("/voices", JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty voices response.");
    }

    public async Task<CreateJobResponse> CreateJobAsync(GenerateJobRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/jobs/generate", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CreateJobResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty job creation response.");
    }

    public async Task<JobResponse> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<JobResponse>($"/jobs/{jobId}", JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty job response.");
    }

    public async Task<JobActionResponse> StopJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/jobs/{jobId}/stop", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<JobActionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty stop response.");
    }

    public async Task<ArtifactResponse> GetArtifactAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ArtifactResponse>($"/jobs/{jobId}/artifact", JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Runtime returned an empty artifact response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(detail) ? response.ReasonPhrase : detail);
    }
}
