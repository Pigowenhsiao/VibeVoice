using VibeVoice.Desktop.Services;
using Xunit;

namespace VibeVoice.Desktop.Tests.Services;

public sealed class ArtifactExportServiceTests
{
    [Fact]
    public async Task ExportAsync_CopiesArtifactIntoConfiguredOutputDirectory()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vibevoice-export-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var sourcePath = Path.Combine(tempRoot, "source.wav");
        await File.WriteAllBytesAsync(sourcePath, new byte[] { 1, 2, 3, 4 });

        var exportRoot = Path.Combine(tempRoot, "exports");
        var service = new ArtifactExportService();

        var exportedPath = await service.ExportAsync(sourcePath, exportRoot);

        Assert.StartsWith(exportRoot, exportedPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(exportedPath));
        Assert.Equal(await File.ReadAllBytesAsync(sourcePath), await File.ReadAllBytesAsync(exportedPath));
    }
}
