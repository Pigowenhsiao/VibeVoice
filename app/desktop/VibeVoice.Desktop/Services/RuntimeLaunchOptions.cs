namespace VibeVoice.Desktop.Services;

public sealed class RuntimeLaunchOptions
{
    public string BaseAddress { get; init; } = "http://127.0.0.1:8765";

    public string PythonExecutable { get; init; } =
        Environment.GetEnvironmentVariable("VIBEVOICE_PYTHON") ?? "python";

    public string ModuleName { get; init; } = "app.runtime.main";

    public string RepoRoot { get; init; } = RepositoryRootLocator.FindFrom(AppContext.BaseDirectory);

    public int StartupAttempts { get; init; } = 12;

    public TimeSpan StartupDelay { get; init; } = TimeSpan.FromMilliseconds(500);
}
