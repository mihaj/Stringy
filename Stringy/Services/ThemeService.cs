using Microsoft.JSInterop;

namespace Stringy.Services;

/// <summary>Tracks the light/dark theme, persisting the choice to localStorage via a tiny JS shim.</summary>
public sealed class ThemeService(IJSRuntime js)
{
    public string Theme { get; private set; } = "light";
    public bool IsDark => Theme == "dark";

    public event Action? OnChange;

    /// <summary>Reads the persisted theme, defaulting to light when the user hasn't chosen one.</summary>
    public async Task InitializeAsync()
    {
        var saved = await js.InvokeAsync<string?>("stringy.getTheme");
        Theme = string.IsNullOrEmpty(saved) ? "light" : saved;
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
