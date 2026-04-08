using VibeVoice.Desktop.Services;
using Xunit;

namespace VibeVoice.Desktop.Tests.Services;

public sealed class DesktopSettingsServiceTests
{
    [Fact]
    public async Task SaveAsync_PersistsAndReloadsLastUsedDefaults()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vibevoice-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var settingsPath = Path.Combine(tempRoot, "desktop-settings.json");
        var service = new DesktopSettingsService(settingsPath);

        var expected = new DesktopSettings
        {
            ModelPath = @"E:\AI Training\VibeVoice\checkpoints",
            OutputDirectory = Path.Combine(tempRoot, "exports"),
            SpeakerCount = 3,
            CfgScale = 1.4,
            LastScript = "Speaker 0: hello",
        };

        await service.SaveAsync(expected);
        var actual = service.Load();

        Assert.Equal(expected.ModelPath, actual.ModelPath);
        Assert.Equal(expected.OutputDirectory, actual.OutputDirectory);
        Assert.Equal(expected.SpeakerCount, actual.SpeakerCount);
        Assert.Equal(expected.CfgScale, actual.CfgScale);
        Assert.Equal(expected.LastScript, actual.LastScript);
    }
}
