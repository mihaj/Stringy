using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stringy;
using Stringy.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Everything runs client-side in the browser. No backend, no telemetry, no tracking.
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ClipboardService>();

await builder.Build().RunAsync();
