using System.IO;
using System.Windows.Media;

namespace VibeVoice.Desktop.Services;

public interface IAudioPlaybackService
{
    Task PlayAsync(string artifactPath, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public sealed class AudioPlaybackService : IAudioPlaybackService, IDisposable
{
    private readonly MediaPlayer _player = new();

    public Task PlayAsync(string artifactPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(artifactPath))
        {
            throw new FileNotFoundException("Artifact not found for playback.", artifactPath);
        }

        _player.Open(new Uri(artifactPath, UriKind.Absolute));
        _player.Play();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _player.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _player.Close();
    }
}
