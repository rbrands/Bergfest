using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using BlazorApp.Client.Utils;
using AzureStaticWebApps.Blazor.Authentication;
using Blazored.Modal;
using Microsoft.Fast.Components.FluentUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BackendApiRepository>();
builder.Services.AddSingleton<AppState>();
builder.Services.AddBlazoredModal();
builder.Services.AddFluentUIComponents();
builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();
