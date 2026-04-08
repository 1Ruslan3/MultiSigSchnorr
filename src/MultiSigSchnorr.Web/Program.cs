using Microsoft.Extensions.Options;
using MultiSigSchnorr.Web.Components;
using MultiSigSchnorr.Web.Services.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiOptions>(
    builder.Configuration.GetSection("Api"));

builder.Services.AddHttpClient<ProtocolSessionsApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
    var baseUrl = options.BaseUrl?.Trim();

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Api:BaseUrl is not configured.");

    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();