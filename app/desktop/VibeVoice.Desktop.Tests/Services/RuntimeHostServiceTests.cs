using System.Diagnostics;
using VibeVoice.Desktop.Models;
using VibeVoice.Desktop.Services;
using Xunit;

namespace VibeVoice.Desktop.Tests.Services;

public sealed class RuntimeHostServiceTests
{
    [Fact]
    public async Task EnsureReadyAsync_DoesNotStartProcess_WhenRuntimeAlreadyResponds()
    {
        var client = new FakeRuntimeApiClient(
            new HealthResponse
            {
                Status = "ok",
                RuntimeVersion = "1.0.0",
                Runtime = new RuntimeInfo { Device = "cpu", PythonVersion = "3.11" },
                Model = new ModelInfo { Status = "idle" },
            });
        var starter = new FakeProcessStarter();
        var service = new RuntimeHostService(
            client,
            new RuntimeLaunchOptions
            {
                RepoRoot = @"E:\AI Training\VibeVoice",
                StartupAttempts = 2,
                StartupDelay = TimeSpan.Zero,
            },
            starter,
            (_, _) => Task.CompletedTask);

        var result = await service.EnsureReadyAsync();

        Assert.False(result.LaunchedInternalRuntime);
        Assert.Equal(0, starter.StartCount);
        Assert.Equal(1, client.HealthChecks);
    }

    [Fact]
    public async Task EnsureReadyAsync_StartsProcessAndRetries_WhenRuntimeIsInitiallyUnavailable()
    {
        var client = new FakeRuntimeApiClient(
            new HttpRequestException("runtime offline"),
            new HealthResponse
            {
                Status = "ok",
                RuntimeVersion = "1.0.0",
                Runtime = new RuntimeInfo { Device = "cpu", PythonVersion = "3.11" },
                Model = new ModelInfo { Status = "loaded", Loaded = true },
            });
        var starter = new FakeProcessStarter();
        var service = new RuntimeHostService(
            client,
            new RuntimeLaunchOptions
            {
                RepoRoot = @"E:\AI Training\VibeVoice",
                StartupAttempts = 3,
                StartupDelay = TimeSpan.Zero,
            },
            starter,
            (_, _) => Task.CompletedTask);

        var result = await service.EnsureReadyAsync();

        Assert.True(result.LaunchedInternalRuntime);
        Assert.Equal(1, starter.StartCount);
        Assert.Equal("python", starter.LastStartInfo?.FileName);
        Assert.Equal("-m app.runtime.main", starter.LastStartInfo?.Arguments);
        Assert.Equal(@"E:\AI Training\VibeVoice", starter.LastStartInfo?.WorkingDirectory);
        Assert.Equal(2, client.HealthChecks);
    }

    private sealed class FakeRuntimeApiClient : IRuntimeApiClient
    {
        private readonly Queue<object> _responses;

        public FakeRuntimeApiClient(params object[] responses)
        {
            _responses = new Queue<object>(responses);
        }

        public int HealthChecks { get; private set; }

        public Task<HealthResponse> GetHealthAsync(CancellationToken cancellationToken = default)
        {
            HealthChecks++;
            var next = _responses.Dequeue();
            return next switch
            {
                Exception ex => Task.FromException<HealthResponse>(ex),
                HealthResponse health => Task.FromResult(health),
                _ => Task.FromException<HealthResponse>(new InvalidOperationException("Unexpected fake response.")),
            };
        }

        public Task<ModelActionResponse> LoadModelAsync(LoadModelRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<VoiceListResponse> GetVoicesAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<CreateJobResponse> CreateJobAsync(GenerateJobRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<JobResponse> GetJobAsync(string jobId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<JobActionResponse> StopJobAsync(string jobId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ArtifactResponse> GetArtifactAsync(string jobId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeProcessStarter : IProcessStarter
    {
        public int StartCount { get; private set; }

        public ProcessStartInfo? LastStartInfo { get; private set; }

        public Process? Start(ProcessStartInfo startInfo)
        {
            StartCount++;
            LastStartInfo = startInfo;
            return null;
        }
    }
}
