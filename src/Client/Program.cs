using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SharpPad.Client;
using SharpPad.Client.Services.AI;
using SharpPad.Client.Services.Auth;
using SharpPad.Client.Services.Caching;
using SharpPad.Client.Services.Components;
using SharpPad.Client.Services.Integrations;
using SharpPad.Client.Services.Library;
using SharpPad.Client.Services.Storage;
using SharpPad.Client.Services.Streaming;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the AI service
builder.Services.AddScoped<ICodeAssistantApiClient, CodeAssistantApiClient>();

// Register CodeExecutionClientService as a Singleton (recommended for SignalR clients)
builder.Services.AddSingleton<CodeExecutionClientService>();


// Register the NuGet search service
builder.Services.AddScoped<IClientCache, ClientCache>();
builder.Services.AddScoped<INuGetSearchService, NuGetSearchService>();

// Register the application Auth service
builder.Services.AddScoped<IAuthClient, AuthClient>();

// Register the Script Library service
builder.Services.AddScoped<ILibraryScriptClient, LibraryScriptClient>();

// Register the local storage service
builder.Services.AddSingleton<ISimpleStorage, LocalStorageService>();

// Register the application Toast service
builder.Services.AddSingleton<ToastService>();

// Register the confirmation dialog service
builder.Services.AddSingleton<ConfirmDialogService>();

await builder.Build().RunAsync();
