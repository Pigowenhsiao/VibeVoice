using System.IO;

namespace VibeVoice.Desktop.Services;

public interface IArtifactExportService
{
    Task<string> ExportAsync(string artifactPath, string outputDirectory, CancellationToken cancellationToken = default);
}

public sealed class ArtifactExportService : IArtifactExportService
{
    public async Task<string> ExportAsync(string artifactPath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(artifactPath))
        {
            throw new FileNotFoundException("Artifact not found for export.", artifactPath);
        }

        Directory.CreateDirectory(outputDirectory);

        var fileName = Path.GetFileName(artifactPath);
        var destinationPath = Path.Combine(outputDirectory, fileName);
        if (Path.GetFullPath(destinationPath).Equals(Path.GetFullPath(artifactPath), StringComparison.OrdinalIgnoreCase))
        {
            var safeName = $"{Path.GetFileNameWithoutExtension(fileName)}-export{Path.GetExtension(fileName)}";
            destinationPath = Path.Combine(outputDirectory, safeName);
        }

        await using var source = File.OpenRead(artifactPath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
        return destinationPath;
    }
}
