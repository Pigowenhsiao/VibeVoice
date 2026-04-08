using System.Text.Json.Serialization;

namespace VibeVoice.Desktop.Models;

public sealed class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("runtime_version")]
    public string RuntimeVersion { get; set; } = string.Empty;

    [JsonPropertyName("runtime")]
    public RuntimeInfo Runtime { get; set; } = new();

    [JsonPropertyName("model")]
    public ModelInfo Model { get; set; } = new();
}

public sealed class RuntimeInfo
{
    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;

    [JsonPropertyName("python_version")]
    public string PythonVersion { get; set; } = string.Empty;

    [JsonPropertyName("ffmpeg_available")]
    public bool FfmpegAvailable { get; set; }

    [JsonPropertyName("checkpoints_root")]
    public string CheckpointsRoot { get; set; } = string.Empty;
}

public sealed class ModelInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("loaded")]
    public bool Loaded { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("device")]
    public string? Device { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public sealed class LoadModelRequest
{
    [JsonPropertyName("model_path")]
    public string ModelPath { get; set; } = string.Empty;

    [JsonPropertyName("inference_steps")]
    public int InferenceSteps { get; set; } = 10;
}

public sealed class ModelActionResponse
{
    [JsonPropertyName("accepted")]
    public bool Accepted { get; set; }

    [JsonPropertyName("model")]
    public ModelInfo Model { get; set; } = new();
}

public sealed class VoicePreset
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}

public sealed class VoiceListResponse
{
    [JsonPropertyName("items")]
    public List<VoicePreset> Items { get; set; } = [];
}

public sealed class SpeakerSelection
{
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;
}

public sealed class GenerateJobRequest
{
    [JsonPropertyName("speaker_count")]
    public int SpeakerCount { get; set; }

    [JsonPropertyName("speakers")]
    public List<SpeakerSelection> Speakers { get; set; } = [];

    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;

    [JsonPropertyName("cfg_scale")]
    public double CfgScale { get; set; }
}

public sealed class RuntimeJobEvent
{
    public RuntimeJobEvent()
    {
    }

    public RuntimeJobEvent(string type, RuntimeJobEventPayload payload)
    {
        Type = type;
        Payload = payload;
    }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public RuntimeJobEventPayload Payload { get; set; } = new();
}

public sealed class RuntimeJobEventPayload
{
    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("progress")]
    public double? Progress { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("artifact_path")]
    public string? ArtifactPath { get; set; }

    [JsonPropertyName("path")]
    public string? ResultPath { get; set; }

    [JsonPropertyName("duration_seconds")]
    public double? DurationSeconds { get; set; }

    public string? EffectiveArtifactPath =>
        !string.IsNullOrWhiteSpace(ResultPath) ? ResultPath : ArtifactPath;
}

public sealed class CreateJobResponse
{
    [JsonPropertyName("accepted")]
    public bool Accepted { get; set; }

    [JsonPropertyName("job")]
    public JobInfo Job { get; set; } = new();
}

public sealed class JobResponse
{
    [JsonPropertyName("job")]
    public JobInfo Job { get; set; } = new();
}

public sealed class JobActionResponse
{
    [JsonPropertyName("accepted")]
    public bool Accepted { get; set; }

    [JsonPropertyName("job")]
    public JobInfo Job { get; set; } = new();
}

public sealed class JobInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("artifact_path")]
    public string? ArtifactPath { get; set; }
}

public sealed class ArtifactResponse
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("duration_seconds")]
    public double? DurationSeconds { get; set; }
}
