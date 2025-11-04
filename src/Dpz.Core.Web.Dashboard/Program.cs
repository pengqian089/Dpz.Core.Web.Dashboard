using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Dpz.Core.Web.Dashboard;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

BaseAddress = builder.Configuration.GetSection("BaseAddress").Get<string>();
if (string.IsNullOrWhiteSpace(BaseAddress))
{
    throw new Exception("BaseAddress is null.");
}

builder
    .Services.AddHttpClient("ServerAPI")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(BaseAddress))
    .AddHttpMessageHandler(sp =>
        sp.GetRequiredService<AuthorizationMessageHandler>()
            .ConfigureHandler(authorizedUrls: [BaseAddress])
    );
builder.Services.AddScoped(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("ServerAPI");
});

var oidcConfig = builder.Configuration.GetSection("OIDC");
builder.Services.AddOidcAuthentication(options =>
{
    oidcConfig.Bind(options.ProviderOptions);
    Console.WriteLine(options.ProviderOptions.ClientId);
    options.ProviderOptions.DefaultScopes.Add("roles");
    options.UserOptions.RoleClaim = "role";
});

builder.Services.AddMudServices();

CdnBaseAddress = builder.Configuration["CDNBaseAddress"];
WebHost = builder.Configuration["SourceSite"];
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(BaseAddress) });

RegisterInject(builder);

var host = builder.Build();

await host.RunAsync();

return;
static void RegisterInject(WebAssemblyHostBuilder builder)
{
    var allTypes = Assembly.GetExecutingAssembly().GetTypes();
    var injectTypes = allTypes.Where(x =>
        x.Namespace == "Dpz.Core.Web.Dashboard.Service" && x.IsInterface
    );
    var implementAssembly = allTypes
        .Where(x =>
            x.Namespace == "Dpz.Core.Web.Dashboard.Service.Impl" && !x.IsAbstract && !x.IsInterface
        )
        .ToList();
    foreach (var injectType in injectTypes)
    {
        var defaultImplementType = implementAssembly.FirstOrDefault(x =>
            injectType.IsAssignableFrom(x)
        );
        if (defaultImplementType != null)
        {
            builder.Services.AddScoped(injectType, defaultImplementType);
        }
    }
}

partial class Program
{
    /// <summary>
    /// web host
    /// </summary>
    public static string WebHost { get; private set; }

    /// <summary>
    /// API base address
    /// </summary>
    public static string BaseAddress { get; private set; }

    /// <summary>
    /// CDN
    /// </summary>
    public static string CdnBaseAddress { get; private set; }
}
