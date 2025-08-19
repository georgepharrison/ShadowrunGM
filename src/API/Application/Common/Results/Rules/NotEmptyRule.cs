namespace ShadowrunGM.API.Application.Common.Results.Rules;

public sealed class NotEmptyRule<T> : IRule<T>
{
    #region Public Methods

    public string? Validate(T value, string displayName) =>
        value switch
        {
            null => $"{displayName} must not be empty",
            string s when string.IsNullOrWhiteSpace(s) => $"{displayName} must not be empty",
            DateTime dt when dt == default => $"{displayName} must not be empty",
            Guid g when g == Guid.Empty => $"{displayName} must not be empty",
            System.Collections.IEnumerable e when !e.Cast<object>().Any() => $"{displayName} must not be empty",
            _ => null
        };

    #endregion Public Methods
}