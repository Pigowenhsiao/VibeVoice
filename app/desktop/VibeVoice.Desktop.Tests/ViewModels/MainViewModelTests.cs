using System.Collections.ObjectModel;
using VibeVoice.Desktop.Models;
using VibeVoice.Desktop.Services;
using VibeVoice.Desktop.ViewModels;
using Xunit;

namespace VibeVoice.Desktop.Tests.ViewModels;

public sealed class MainViewModelTests
{
    [Fact]
    public async Task GenerateCommand_ConsumesTypedProgressEvents_AndEnablesResultActions()
    {
        var client = new FakeRuntimeApiClient();
        var runtimeHost = new FakeRuntimeHostService();
        var eventClient = new FakeRuntimeEventStreamClient(
            new RuntimeJobEvent("job_state_changed", new RuntimeJobEventPayload { JobId = "job-123", State = "running", Progress = 0.55, Message = "Generating" }),
            new RuntimeJobEvent("progress_updated", new RuntimeJobEventPayload { JobId = "job-123", Progress = 0.85, Message = "Saving artifact" }),
            new RuntimeJobEvent("artifact_ready", new RuntimeJobEventPayload { JobId = "job-123", ArtifactPath = @"E:\AI Training\VibeVoice\outputs\job-123.wav", DurationSeconds = 12.5 }),
            new RuntimeJobEvent("job_state_changed", new RuntimeJobEventPayload { JobId = "job-123", State = "completed", Progress = 1.0, Message = "Completed", ArtifactPath = @"E:\AI Training\VibeVoice\outputs\job-123.wav" }));
        var playback = new FakeAudioPlaybackService();
        var export = new FakeArtifactExportService();
        var settings = new FakeDesktopSettingsService(new DesktopSettings
        {
            ModelPath = @"E:\AI Training\VibeVoice\checkpoints",
            OutputDirectory = @"E:\AI Training\VibeVoice\outputs\exports",
            SpeakerCount = 2,
            CfgScale = 1.3,
            LastScript = "Speaker 0: hello",
        });

        var viewModel = new MainViewModel(client, runtimeHost, eventClient, playback, export, settings);
        await Task.Delay(50);

        viewModel.SelectedVoice1 = viewModel.Voices[0];
        viewModel.SelectedVoice2 = viewModel.Voices[1];
        viewModel.ScriptText = "Speaker 0: hello\nSpeaker 1: world";

        viewModel.GenerateCommand.Execute(null);
        await Task.Delay(150);

        Assert.Equal("completed", viewModel.JobState);
        Assert.Equal(1.0, viewModel.JobProgress, 3);
        Assert.Equal(@"E:\AI Training\VibeVoice\outputs\job-123.wav", viewModel.ArtifactPath);
        Assert.Contains("12.5", viewModel.ResultSummary);
        Assert.True(viewModel.PlayResultCommand.CanExecute(null));
        Assert.True(viewModel.ExportResultCommand.CanExecute(null));

        viewModel.PlayResultCommand.Execute(null);
        await Task.Delay(25);
        Assert.Equal(viewModel.ArtifactPath, playback.LastPlayedPath);

        viewModel.ExportResultCommand.Execute(null);
        await Task.Delay(25);
        Assert.Equal(Path.Combine(settings.Current.OutputDirectory, "job-123.wav"), export.LastExportedPath);
    }

    private sealed class FakeRuntimeApiClient : IRuntimeApiClient
    {
        public Task<HealthResponse> GetHealthAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthResponse
            {
                Status = "ok",
                RuntimeVersion = "1.0.0",
                Runtime = new RuntimeInfo
                {
                    Device = "cpu",
                    PythonVersion = "3.11",
                    FfmpegAvailable = true,
                    CheckpointsRoot = @"E:\AI Training\VibeVoice\checkpoints",
                },
                Model = new ModelInfo
                {
                    Loaded = true,
                    Status = "loaded",
                    Message = "ready",
                },
            });
        }

        public Task<ModelActionResponse> LoadModelAsync(LoadModelRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ModelActionResponse
            {
                Accepted = true,
                Model = new ModelInfo { Loaded = true, Status = "loaded", Message = "ready" },
            });
        }

        public Task<VoiceListResponse> GetVoicesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new VoiceListResponse
            {
                Items =
                [
                    new VoicePreset { Id = "speaker-a", DisplayName = "Speaker A", Path = @"E:\voices\a.wav" },
                    new VoicePreset { Id = "speaker-b", DisplayName = "Speaker B", Path = @"E:\voices\b.wav" },
                ],
            });
        }

        public Task<CreateJobResponse> CreateJobAsync(GenerateJobRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CreateJobResponse
            {
                Accepted = true,
                Job = new JobInfo
                {
                    Id = "job-123",
                    State = "queued",
                    Progress = 0.0,
                    Message = "Queued",
                },
            });
        }

        public Task<JobResponse> GetJobAsync(string jobId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<JobActionResponse> StopJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new JobActionResponse
            {
                Accepted = true,
                Job = new JobInfo { Id = jobId, State = "stopping", Progress = 0.2, Message = "Stopping" },
            });
        }

        public Task<ArtifactResponse> GetArtifactAsync(string jobId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ArtifactResponse
            {
                JobId = jobId,
                Path = @"E:\AI Training\VibeVoice\outputs\job-123.wav",
                DurationSeconds = 12.5,
            });
        }
    }

    private sealed class FakeRuntimeHostService : IRuntimeHostService
    {
        public Task<RuntimeReadyResult> EnsureReadyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RuntimeReadyResult(
                new HealthResponse
                {
                    Status = "ok",
                    RuntimeVersion = "1.0.0",
                    Runtime = new RuntimeInfo
                    {
                        Device = "cpu",
                        PythonVersion = "3.11",
                        FfmpegAvailable = true,
                        CheckpointsRoot = @"E:\AI Training\VibeVoice\checkpoints",
                    },
                    Model = new ModelInfo
                    {
                        Loaded = true,
                        Status = "loaded",
                        Message = "ready",
                    },
                },
                false));
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeRuntimeEventStreamClient(params RuntimeJobEvent[] events) : IRuntimeEventStreamClient
    {
        private readonly ReadOnlyCollection<RuntimeJobEvent> _events = Array.AsReadOnly(events);

        public async IAsyncEnumerable<RuntimeJobEvent> StreamJobEventsAsync(string jobId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var item in _events)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(5, cancellationToken);
                yield return item;
            }
        }
    }

    private sealed class FakeAudioPlaybackService : IAudioPlaybackService
    {
        public string? LastPlayedPath { get; private set; }

        public Task PlayAsync(string artifactPath, CancellationToken cancellationToken = default)
        {
            LastPlayedPath = artifactPath;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeArtifactExportService : IArtifactExportService
    {
        public string? LastExportedPath { get; private set; }

        public Task<string> ExportAsync(string artifactPath, string outputDirectory, CancellationToken cancellationToken = default)
        {
            LastExportedPath = Path.Combine(outputDirectory, Path.GetFileName(artifactPath));
            return Task.FromResult(LastExportedPath);
        }
    }

    private sealed class FakeDesktopSettingsService(DesktopSettings initialSettings) : IDesktopSettingsService
    {
        public DesktopSettings Current { get; private set; } = initialSettings;

        public DesktopSettings Load() => Current;

        public Task SaveAsync(DesktopSettings settings, CancellationToken cancellationToken = default)
        {
            Current = settings;
            return Task.CompletedTask;
        }
    }
}
