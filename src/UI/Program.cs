using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using ShadowrunGM.ApiSdk;
using ShadowrunGM.UI;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddShadowrunGmApiSdk(options => builder.Configuration.Bind(nameof(ShadowrunGmApiOptions), options));

WebAssemblyHost app = builder.Build();

app.Services.UseShadowrunGmApiSdk();

await app.RunAsync();