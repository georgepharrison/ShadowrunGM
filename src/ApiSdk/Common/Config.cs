namespace ShadowrunGM.ApiSdk.Common;

public sealed class Config
{
    #region Private Members

    private static IServiceResolver? _serviceResolver;

    #endregion Private Members

    #region Public Properties

    public static IServiceResolver ServiceResolver
    {
        get => _serviceResolver ?? throw new InvalidOperationException("ServiceResolver is not initialized.");
        set => _serviceResolver = value ?? throw new ArgumentNullException(nameof(value), "ServiceResolver cannot be null.");
    }

    #endregion Public Properties
}