using System.IO;
using System.Text.Json;

namespace VibeVoice.Desktop.Services;

public sealed class DesktopSettings
{
    public string ModelPath { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public int SpeakerCount { get; set; } = 2;
    public double CfgScale { get; set; } = 1.3;
    public string LastScript { get; set; } = string.Empty;
}

public interface IDesktopSettingsService
{
    DesktopSettings Load();
    Task SaveAsync(DesktopSettings settings, CancellationToken cancellationToken = default);
}

public sealed class DesktopSettingsService : IDesktopSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private readonly string _settingsPath;

    public DesktopSettingsService(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? GetDefaultSettingsPath();
    }

    public DesktopSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            return BuildDefaultSettings();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<DesktopSettings>(File.ReadAllText(_settingsPath), JsonOptions) ?? new DesktopSettings();
            return Normalize(settings);
        }
        catch
        {
            return BuildDefaultSettings();
        }
    }

    public async Task SaveAsync(DesktopSettings settings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = Normalize(settings);
        var directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var content = JsonSerializer.Serialize(normalized, JsonOptions);
        await File.WriteAllTextAsync(_settingsPath, content, cancellationToken);
    }

    private static DesktopSettings Normalize(DesktopSettings settings)
    {
        var defaults = BuildDefaultSettings();
        return new DesktopSettings
        {
            ModelPath = string.IsNullOrWhiteSpace(settings.ModelPath) ? defaults.ModelPath : settings.ModelPath,
            OutputDirectory = string.IsNullOrWhiteSpace(settings.OutputDirectory) ? defaults.OutputDirectory : settings.OutputDirectory,
            SpeakerCount = Math.Clamp(settings.SpeakerCount, 1, 4),
            CfgScale = settings.CfgScale is < 1.0 or > 2.0 ? defaults.CfgScale : settings.CfgScale,
            LastScript = string.IsNullOrWhiteSpace(settings.LastScript) ? defaults.LastScript : settings.LastScript,
        };
    }

    private static DesktopSettings BuildDefaultSettings()
    {
        var repoRoot = RepositoryRootLocator.FindFrom(AppContext.BaseDirectory);
        return new DesktopSettings
        {
            ModelPath = Path.Combine(repoRoot, "checkpoints"),
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VibeVoice", "Exports"),
            SpeakerCount = 2,
            CfgScale = 1.3,
            LastScript = "Speaker 0: Welcome to the VibeVoice Windows desktop app.\nSpeaker 1: This version starts the local runtime automatically when needed.",
        };
    }

    private static string GetDefaultSettingsPath()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VibeVoice");
        return Path.Combine(root, "desktop-settings.json");
    }
}
