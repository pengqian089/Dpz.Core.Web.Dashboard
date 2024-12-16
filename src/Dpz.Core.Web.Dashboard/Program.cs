using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Dpz.Core.Web.Dashboard;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components.Web;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

BaseAddress = builder.Configuration.GetSection("BaseAddress").Get<string>();
CdnBaseAddress = builder.Configuration["CDNBaseAddress"];
WebHost = builder.Configuration["SourceSite"];
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(BaseAddress) });

RegisterInject(builder);

var host = builder.Build();

var authenticationService = host.Services.GetRequiredService<IAuthenticationService>();
await authenticationService.InitializeAsync();

await host.RunAsync();


static void RegisterInject(WebAssemblyHostBuilder builder)
{
    var allTypes = Assembly.GetExecutingAssembly().GetTypes();
    var injectTypes = allTypes
        .Where(x => x.Namespace == "Dpz.Core.Web.Dashboard.Service" && x.IsInterface);
    var implementAssembly = allTypes
        .Where(x => x.Namespace == "Dpz.Core.Web.Dashboard.Service.Impl" && !x.IsAbstract && !x.IsInterface)
        .ToList();
    foreach (var injectType in injectTypes)
    {
        var defaultImplementType = implementAssembly.FirstOrDefault(x => injectType.IsAssignableFrom(x));
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
