using Microsoft.Extensions.DependencyInjection;

namespace ShadowrunGM.ApiSdk.Common;

public interface IServiceResolver
{
    #region Public Methods

    TService GetRequiredService<TService>() where TService : class;

    TService? GetService<TService>() where TService : class;

    #endregion Public Methods
}

public class ServiceResolver(IServiceScopeFactory serviceScopeFactory) : IServiceResolver
{
    #region Private Members

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    #endregion Private Members

    #region Public Methods

    public TService GetRequiredService<TService>() where TService : class
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    public TService? GetService<TService>() where TService : class
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetService<TService>();
    }

    #endregion Public Methods
}