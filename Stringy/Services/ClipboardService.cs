using Microsoft.JSInterop;

namespace Stringy.Services;

/// <summary>Copies text to and reads text from the system clipboard. Reports failure when the browser blocks the request.</summary>
public sealed class ClipboardService(IJSRuntime js)
{
    public async Task<bool> CopyAsync(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return await js.InvokeAsync<bool>("stringy.copy", text);
    }

    public async Task<string?> PasteAsync()
        => await js.InvokeAsync<string?>("stringy.paste");
}
