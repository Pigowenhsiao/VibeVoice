using System.IO;
using System.Collections.ObjectModel;
using VibeVoice.Desktop.Models;
using VibeVoice.Desktop.Services;

namespace VibeVoice.Desktop.ViewModels;

public sealed class MainViewModel : BaseViewModel, IDisposable
{
    private readonly IRuntimeApiClient _client;
    private readonly IRuntimeHostService _runtimeHost;
    private readonly IRuntimeEventStreamClient _eventClient;
    private readonly IAudioPlaybackService _playbackService;
    private readonly IArtifactExportService _artifactExportService;
    private readonly IDesktopSettingsService _settingsService;
    private DesktopSettings _currentSettings;
    private CancellationTokenSource? _jobEventsCts;
    private string? _currentJobId;
    private string _runtimeStatus = "Runtime not checked yet.";
    private string _runtimeSummary = "No runtime summary yet.";
    private string _modelPath = string.Empty;
    private string _modelStatus = "No model loaded.";
    private string _scriptText = string.Empty;
    private string _jobState = "idle";
    private string _jobMessage = "No job running.";
    private string _artifactPath = string.Empty;
    private double _jobProgress;
    private string _elapsedTime = "00:00";
    private string _resultSummary = "No result yet.";
    private string _playbackStatus = "Playback idle.";
    private string _outputDirectory = string.Empty;
    private string _settingsStatus = "Settings not saved yet.";
    private string _errorMessage = string.Empty;
    private int _speakerCount = 2;
    private double _cfgScale = 1.3;
    private VoicePreset? _selectedVoice1;
    private VoicePreset? _selectedVoice2;
    private VoicePreset? _selectedVoice3;
    private VoicePreset? _selectedVoice4;
    private bool _isStopping;
    private bool _isModelLoaded;
    private DateTimeOffset? _jobStartedAt;

    public MainViewModel()
        : this(
            CreateDefaultClient(),
            CreateDefaultRuntimeHost(),
            CreateDefaultEventClient(),
            new AudioPlaybackService(),
            new ArtifactExportService(),
            new DesktopSettingsService())
    {
    }

    public MainViewModel(
        IRuntimeApiClient client,
        IRuntimeHostService runtimeHost,
        IRuntimeEventStreamClient eventClient,
        IAudioPlaybackService playbackService,
        IArtifactExportService artifactExportService,
        IDesktopSettingsService settingsService)
    {
        _client = client;
        _runtimeHost = runtimeHost;
        _eventClient = eventClient;
        _playbackService = playbackService;
        _artifactExportService = artifactExportService;
        _settingsService = settingsService;
        _currentSettings = _settingsService.Load();
        _modelPath = _currentSettings.ModelPath;
        _scriptText = _currentSettings.LastScript;
        _speakerCount = _currentSettings.SpeakerCount;
        _cfgScale = _currentSettings.CfgScale;
        _outputDirectory = _currentSettings.OutputDirectory;

        Voices = new ObservableCollection<VoicePreset>();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        LoadModelCommand = new AsyncRelayCommand(LoadModelAsync, () => !string.IsNullOrWhiteSpace(ModelPath));
        GenerateCommand = new AsyncRelayCommand(GenerateAsync, CanGenerate);
        StopCommand = new AsyncRelayCommand(StopAsync, () => !string.IsNullOrWhiteSpace(_currentJobId) && !_isStopping && JobState is not "completed" and not "failed" and not "cancelled");
        PlayResultCommand = new AsyncRelayCommand(PlayResultAsync, () => !string.IsNullOrWhiteSpace(ArtifactPath));
        StopPlaybackCommand = new AsyncRelayCommand(StopPlaybackAsync, () => !string.IsNullOrWhiteSpace(ArtifactPath));
        ExportResultCommand = new AsyncRelayCommand(ExportResultAsync, () => !string.IsNullOrWhiteSpace(ArtifactPath) && !string.IsNullOrWhiteSpace(OutputDirectory));
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);

        _ = RefreshAsync();
    }

    public ObservableCollection<VoicePreset> Voices { get; }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand LoadModelCommand { get; }
    public AsyncRelayCommand GenerateCommand { get; }
    public AsyncRelayCommand StopCommand { get; }
    public AsyncRelayCommand PlayResultCommand { get; }
    public AsyncRelayCommand StopPlaybackCommand { get; }
    public AsyncRelayCommand ExportResultCommand { get; }
    public AsyncRelayCommand SaveSettingsCommand { get; }

    public string RuntimeStatus
    {
        get => _runtimeStatus;
        set => SetProperty(ref _runtimeStatus, value);
    }

    public string RuntimeSummary
    {
        get => _runtimeSummary;
        set => SetProperty(ref _runtimeSummary, value);
    }

    public string ModelPath
    {
        get => _modelPath;
        set
        {
            if (SetProperty(ref _modelPath, value))
            {
                LoadModelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ModelStatus
    {
        get => _modelStatus;
        set => SetProperty(ref _modelStatus, value);
    }

    public string ScriptText
    {
        get => _scriptText;
        set
        {
            if (SetProperty(ref _scriptText, value))
            {
                GenerateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int SpeakerCount
    {
        get => _speakerCount;
        set
        {
            if (SetProperty(ref _speakerCount, Math.Clamp(value, 1, 4)))
            {
                GenerateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public double CfgScale
    {
        get => _cfgScale;
        set => SetProperty(ref _cfgScale, value);
    }

    public VoicePreset? SelectedVoice1
    {
        get => _selectedVoice1;
        set
        {
            if (SetProperty(ref _selectedVoice1, value))
            {
                GenerateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public VoicePreset? SelectedVoice2
    {
        get => _selectedVoice2;
        set
        {
            if (SetProperty(ref _selectedVoice2, value))
            {
                GenerateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public VoicePreset? SelectedVoice3
    {
        get => _selectedVoice3;
        set
        {
            if (SetProperty(ref _selectedVoice3, value))
            {
                GenerateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public VoicePreset? SelectedVoice4
    {
        get => _selectedVoice4;
        set
        {
            if (SetProperty(ref _selectedVoice4, value))
            {
                GenerateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string JobState
    {
        get => _jobState;
        set
        {
            if (SetProperty(ref _jobState, value))
            {
                StopCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string JobMessage
    {
        get => _jobMessage;
        set => SetProperty(ref _jobMessage, value);
    }

    public string ArtifactPath
    {
        get => _artifactPath;
        set
        {
            if (SetProperty(ref _artifactPath, value))
            {
                PlayResultCommand.RaiseCanExecuteChanged();
                StopPlaybackCommand.RaiseCanExecuteChanged();
                ExportResultCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public double JobProgress
    {
        get => _jobProgress;
        set => SetProperty(ref _jobProgress, value);
    }

    public string ElapsedTime
    {
        get => _elapsedTime;
        set => SetProperty(ref _elapsedTime, value);
    }

    public string ResultSummary
    {
        get => _resultSummary;
        set => SetProperty(ref _resultSummary, value);
    }

    public string PlaybackStatus
    {
        get => _playbackStatus;
        set => SetProperty(ref _playbackStatus, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            if (SetProperty(ref _outputDirectory, value))
            {
                ExportResultCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SettingsStatus
    {
        get => _settingsStatus;
        set => SetProperty(ref _settingsStatus, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool CanGenerate()
    {
        return _isModelLoaded && !string.IsNullOrWhiteSpace(ScriptText) && BuildSpeakerSelections().Count >= SpeakerCount;
    }

    private async Task RefreshAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            var readiness = await _runtimeHost.EnsureReadyAsync();
            var health = readiness.Health;
            RuntimeStatus = readiness.LaunchedInternalRuntime
                ? $"Runtime {health.Status} | v{health.RuntimeVersion} | desktop managed startup completed"
                : $"Runtime {health.Status} | v{health.RuntimeVersion} | already available";
            RuntimeSummary = $"{health.Runtime.Device} | Python {health.Runtime.PythonVersion} | ffmpeg {(health.Runtime.FfmpegAvailable ? "ready" : "missing")}";
            ModelStatus = $"{health.Model.Status} | {health.Model.Message}";
            _isModelLoaded = health.Model.Loaded;
            GenerateCommand.RaiseCanExecuteChanged();
            SettingsStatus = $"Output folder | {OutputDirectory}";

            var voices = await _client.GetVoicesAsync();
            Voices.Clear();
            foreach (var voice in voices.Items)
            {
                Voices.Add(voice);
            }

            SelectedVoice1 ??= Voices.ElementAtOrDefault(0);
            SelectedVoice2 ??= Voices.ElementAtOrDefault(1);
            SelectedVoice3 ??= Voices.ElementAtOrDefault(2);
            SelectedVoice4 ??= Voices.ElementAtOrDefault(3);
        }
        catch (Exception ex)
        {
            RuntimeStatus = "Runtime unavailable.";
            RuntimeSummary = "Desktop runtime startup failed.";
            ErrorMessage = ex.Message;
        }
    }

    private async Task LoadModelAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ModelStatus = "Loading model...";
            var response = await _client.LoadModelAsync(new LoadModelRequest
            {
                ModelPath = ModelPath,
                InferenceSteps = 10,
            });
            ModelStatus = $"{response.Model.Status} | {response.Model.Message}";
            _isModelLoaded = response.Model.Loaded;
            GenerateCommand.RaiseCanExecuteChanged();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ModelStatus = "Model load failed.";
            ErrorMessage = ex.Message;
        }
    }

    private async Task GenerateAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ArtifactPath = string.Empty;
            JobProgress = 0.0;
            ElapsedTime = "00:00";
            ResultSummary = "Waiting for artifact.";
            PlaybackStatus = "Playback idle.";
            JobState = "queued";
            JobMessage = "Submitting job...";
            _jobStartedAt = DateTimeOffset.UtcNow;

            var response = await _client.CreateJobAsync(new GenerateJobRequest
            {
                SpeakerCount = SpeakerCount,
                Speakers = BuildSpeakerSelections(),
                Script = ScriptText,
                CfgScale = CfgScale,
            });

            _currentJobId = response.Job.Id;
            JobState = response.Job.State;
            JobProgress = response.Job.Progress;
            JobMessage = response.Job.Message ?? "Job accepted.";
            StartJobEventListening(_currentJobId);
            StopCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            JobState = "failed";
            JobMessage = "Job submission failed.";
            ErrorMessage = ex.Message;
        }
    }

    private async Task StopAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentJobId))
        {
            return;
        }

        try
        {
            _isStopping = true;
            StopCommand.RaiseCanExecuteChanged();
            var response = await _client.StopJobAsync(_currentJobId);
            JobState = response.Job.State;
            JobProgress = response.Job.Progress;
            JobMessage = response.Job.Message ?? "Stop requested.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            _isStopping = false;
            StopCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task PlayResultAsync()
    {
        await _playbackService.PlayAsync(ArtifactPath);
        PlaybackStatus = $"Playing | {Path.GetFileName(ArtifactPath)}";
    }

    private async Task StopPlaybackAsync()
    {
        await _playbackService.StopAsync();
        PlaybackStatus = "Playback stopped.";
    }

    private async Task ExportResultAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            var exportedPath = await _artifactExportService.ExportAsync(ArtifactPath, OutputDirectory);
            ResultSummary = $"Exported copy | {exportedPath}";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task SaveSettingsAsync()
    {
        var snapshot = BuildSettingsSnapshot();
        await _settingsService.SaveAsync(snapshot);
        _currentSettings = snapshot;
        SettingsStatus = $"Saved defaults | {snapshot.OutputDirectory}";
    }

    private void StartJobEventListening(string jobId)
    {
        _jobEventsCts?.Cancel();
        _jobEventsCts?.Dispose();
        _jobEventsCts = new CancellationTokenSource();
        _ = ObserveJobAsync(jobId, _jobEventsCts.Token);
    }

    private async Task ObserveJobAsync(string jobId, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var runtimeEvent in _eventClient.StreamJobEventsAsync(jobId, cancellationToken))
            {
                ApplyRuntimeEvent(runtimeEvent);
            }

            if (JobState == "completed" && string.IsNullOrWhiteSpace(ArtifactPath))
            {
                var artifact = await _client.GetArtifactAsync(jobId, cancellationToken);
                ArtifactPath = artifact.Path;
                ResultSummary = artifact.DurationSeconds.HasValue
                    ? $"Artifact ready | {artifact.DurationSeconds.Value:0.0}s"
                    : $"Artifact ready | {Path.GetFileName(artifact.Path)}";
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void ApplyRuntimeEvent(RuntimeJobEvent runtimeEvent)
    {
        var payload = runtimeEvent.Payload;
        switch (runtimeEvent.Type)
        {
            case "job_state_changed":
                if (!string.IsNullOrWhiteSpace(payload.State))
                {
                    JobState = payload.State;
                }

                if (payload.Progress.HasValue)
                {
                    JobProgress = payload.Progress.Value;
                }

                if (!string.IsNullOrWhiteSpace(payload.Message))
                {
                    JobMessage = payload.Message;
                }

                if (!string.IsNullOrWhiteSpace(payload.EffectiveArtifactPath))
                {
                    ArtifactPath = payload.EffectiveArtifactPath!;
                }

                UpdateElapsedTime();
                if (JobState is "completed" or "failed" or "cancelled")
                {
                    if (JobState == "completed")
                    {
                        if (string.IsNullOrWhiteSpace(ResultSummary) || ResultSummary == "Waiting for artifact.")
                        {
                            ResultSummary = string.IsNullOrWhiteSpace(ArtifactPath)
                                ? "Artifact completed."
                                : $"Artifact ready | {Path.GetFileName(ArtifactPath)}";
                        }
                    }
                    else if (JobState == "failed")
                    {
                        ResultSummary = "Generation failed. Review the error panel.";
                    }
                    else
                    {
                        ResultSummary = "Generation cancelled before completion.";
                    }
                }

                break;

            case "progress_updated":
                if (payload.Progress.HasValue)
                {
                    JobProgress = payload.Progress.Value;
                }

                if (!string.IsNullOrWhiteSpace(payload.Message))
                {
                    JobMessage = payload.Message;
                }

                UpdateElapsedTime();
                break;

            case "artifact_ready":
                ArtifactPath = payload.EffectiveArtifactPath ?? ArtifactPath;
                ResultSummary = payload.DurationSeconds.HasValue
                    ? $"Artifact ready | {payload.DurationSeconds.Value:0.0}s"
                    : $"Artifact ready | {Path.GetFileName(ArtifactPath)}";
                break;

            case "warning":
                JobMessage = payload.Message ?? JobMessage;
                break;

            case "error":
                ErrorMessage = payload.Message ?? "Runtime reported an unknown error.";
                break;
        }
    }

    private void UpdateElapsedTime()
    {
        if (_jobStartedAt is null)
        {
            ElapsedTime = "00:00";
            return;
        }

        var elapsed = DateTimeOffset.UtcNow - _jobStartedAt.Value;
        ElapsedTime = $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}";
    }

    private List<SpeakerSelection> BuildSpeakerSelections()
    {
        var voices = new[] { SelectedVoice1, SelectedVoice2, SelectedVoice3, SelectedVoice4 };
        var selections = new List<SpeakerSelection>();
        for (var index = 0; index < SpeakerCount; index++)
        {
            if (voices[index] is null)
            {
                continue;
            }

            selections.Add(new SpeakerSelection
            {
                Slot = index + 1,
                VoiceId = voices[index]!.Id,
            });
        }

        return selections;
    }

    private DesktopSettings BuildSettingsSnapshot()
    {
        return new DesktopSettings
        {
            ModelPath = ModelPath,
            OutputDirectory = OutputDirectory,
            SpeakerCount = SpeakerCount,
            CfgScale = CfgScale,
            LastScript = ScriptText,
        };
    }

    public void Dispose()
    {
        _jobEventsCts?.Cancel();
        _jobEventsCts?.Dispose();
        if (_playbackService is IDisposable disposablePlayback)
        {
            disposablePlayback.Dispose();
        }

        _runtimeHost.Dispose();
    }

    private static IRuntimeApiClient CreateDefaultClient()
    {
        return new RuntimeApiClient("http://127.0.0.1:8765");
    }

    private static IRuntimeHostService CreateDefaultRuntimeHost()
    {
        var client = CreateDefaultClient();
        return new RuntimeHostService(client, new RuntimeLaunchOptions());
    }

    private static IRuntimeEventStreamClient CreateDefaultEventClient()
    {
        return new RuntimeEventStreamClient("http://127.0.0.1:8765");
    }
}
