using Microsoft.JSInterop;

namespace Stringy.Services;

/// <summary>Copies text to the system clipboard. Returns false if the browser blocks the request.</summary>
public sealed class ClipboardService(IJSRuntime js)
{
    public async Task<bool> CopyAsync(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return await js.InvokeAsync<bool>("stringy.copy", text);
    }
}
