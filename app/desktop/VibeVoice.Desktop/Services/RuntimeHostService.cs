using System.Diagnostics;
using VibeVoice.Desktop.Models;

namespace VibeVoice.Desktop.Services;

public interface IProcessStarter
{
    Process? Start(ProcessStartInfo startInfo);
}

public interface IRuntimeHostService : IDisposable
{
    Task<RuntimeReadyResult> EnsureReadyAsync(CancellationToken cancellationToken = default);
}

public sealed record RuntimeReadyResult(HealthResponse Health, bool LaunchedInternalRuntime);

public sealed class RuntimeHostService : IRuntimeHostService
{
    private readonly IRuntimeApiClient _client;
    private readonly RuntimeLaunchOptions _options;
    private readonly IProcessStarter _processStarter;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private Process? _managedProcess;

    public RuntimeHostService(
        IRuntimeApiClient client,
        RuntimeLaunchOptions options,
        IProcessStarter? processStarter = null,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        _client = client;
        _options = options;
        _processStarter = processStarter ?? new DefaultProcessStarter();
        _delayAsync = delayAsync ?? Task.Delay;
    }

    public async Task<RuntimeReadyResult> EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthy = await _client.GetHealthAsync(cancellationToken);
            return new RuntimeReadyResult(healthy, false);
        }
        catch
        {
            StartManagedRuntime();
        }

        for (var attempt = 0; attempt < _options.StartupAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var healthy = await _client.GetHealthAsync(cancellationToken);
                return new RuntimeReadyResult(healthy, true);
            }
            catch when (attempt < _options.StartupAttempts - 1)
            {
                await _delayAsync(_options.StartupDelay, cancellationToken);
            }
        }

        var finalHealth = await _client.GetHealthAsync(cancellationToken);
        return new RuntimeReadyResult(finalHealth, true);
    }

    public void Dispose()
    {
        if (_managedProcess is null)
        {
            return;
        }

        try
        {
            if (!_managedProcess.HasExited)
            {
                _managedProcess.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best effort cleanup on app shutdown.
        }
        finally
        {
            _managedProcess.Dispose();
            _managedProcess = null;
        }
    }

    private void StartManagedRuntime()
    {
        if (_managedProcess is not null && !_managedProcess.HasExited)
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.PythonExecutable,
            Arguments = $"-m {_options.ModuleName}",
            WorkingDirectory = _options.RepoRoot,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        _managedProcess = _processStarter.Start(startInfo);
    }

    private sealed class DefaultProcessStarter : IProcessStarter
    {
        public Process? Start(ProcessStartInfo startInfo)
        {
            return Process.Start(startInfo);
        }
    }
}
