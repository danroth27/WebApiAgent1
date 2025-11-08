using BlazorApp1.Components;
using Microsoft.Agents.AI.AGUI;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("webapiagent1", httpClient => {
    httpClient.BaseAddress = new Uri("https+http://webapiagent1");
});

builder.Services.AddChatClient(sp => new AGUIChatClient(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("webapiagent1"), "ag-ui"));

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
