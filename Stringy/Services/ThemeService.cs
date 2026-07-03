using Microsoft.JSInterop;

namespace Stringy.Services;

/// <summary>Tracks the light/dark theme, persisting the choice to localStorage via a tiny JS shim.</summary>
public sealed class ThemeService(IJSRuntime js)
{
    public string Theme { get; private set; } = "dark";
    public bool IsDark => Theme == "dark";

    public event Action? OnChange;

    /// <summary>Reads the persisted theme (or the OS preference on first visit).</summary>
    public async Task InitializeAsync()
    {
        var saved = await js.InvokeAsync<string?>("stringy.getTheme");
        if (string.IsNullOrEmpty(saved))
        {
            var prefersDark = await js.InvokeAsync<bool>("stringy.prefersDark");
            saved = prefersDark ? "dark" : "light";
        }
        Theme = saved;
        await ApplyAsync();
    }

    public async Task ToggleAsync()
    {
        Theme = IsDark ? "light" : "dark";
        await ApplyAsync();
    }

    private async Task ApplyAsync()
    {
        await js.InvokeVoidAsync("stringy.setTheme", Theme);
        OnChange?.Invoke();
    }
}
