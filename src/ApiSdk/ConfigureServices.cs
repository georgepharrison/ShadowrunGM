using Microsoft.Extensions.DependencyInjection;
using FlowRight.Cqrs.Http;

namespace ShadowrunGM.ApiSdk;

public static class ConfigureServices
{
    #region Public Methods

    public static IServiceCollection AddShadowrunGmApiSdk(this IServiceCollection services, Action<ShadowrunGmApiOptions> createOptions)
    {
        ArgumentNullException.ThrowIfNull(createOptions);

        ShadowrunGmApiOptions options = new();
        createOptions(options);

        if (options.BaseAddress == null)
        {
            throw new InvalidOperationException("BaseAddress must be configured in ShadowrunGmApiOptions.");
        }

        // FlowRight.Cqrs.Http source generator provides this method automatically!
        return services.AddFlowRightCqrs(options.BaseAddress);
    }

    public static IServiceProvider UseShadowrunGmApiSdk(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        
        // FlowRight.Cqrs.Http source generator provides this method automatically!
        return serviceProvider.UseFlowRightCqrs();
    }

    #endregion Public Methods
}