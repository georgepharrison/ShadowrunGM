using Microsoft.Extensions.DependencyInjection;
using ShadowrunGM.ApiSdk.Common;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ShadowrunGM.ApiSdk;

public static class ConfigureServices
{
    #region Public Methods

    public static IServiceCollection AddShadowrunGmApiSdk(this IServiceCollection services, Action<ShadowrunGmApiOptions> createOptions)
    {
        ArgumentNullException.ThrowIfNull(createOptions);

        ShadowrunGmApiOptions options = new();
        createOptions(options);

        services.AddHttpClient()
            .AddSingleton<IServiceResolver, ServiceResolver>()
            .AddSingleton(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = JsonTypeInfoResolver.Combine(),
                WriteIndented = true
            });

        services.RegisterHttpClients(options);

        return services;
    }

    public static IServiceProvider UseShadowrunGmApiSdk(this IServiceProvider serviceProvider)
    {
        Config.ServiceResolver = serviceProvider.GetRequiredService<IServiceResolver>();

        return serviceProvider;
    }

    #endregion Public Methods
}