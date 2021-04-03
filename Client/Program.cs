using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorSurveys.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorSurveys.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });

            builder.Services.AddHttpClient<SurveyHttpClient>(client => client.BaseAddress = baseAddress);

            builder.Services.AddSingleton<HubConnection>(sp =>
            {
                var navigationManager = sp.GetRequiredService<NavigationManager>();
                return new HubConnectionBuilder().WithUrl(navigationManager.ToAbsoluteUri("/surveyhub")).WithAutomaticReconnect().Build();
            });

            await builder.Build().RunAsync();
        }
    }
}
