namespace ShadowrunGM.API.Application.Common.Results.Rules;

/// <summary>\n/// Validation rule that ensures a value is not considered \"empty\" according to type-specific logic.\n/// Handles nulls, empty strings, default values, empty GUIDs, and empty collections.\n/// </summary>\n/// <typeparam name=\"T\">The type of value being validated.</typeparam>\n/// <example>\n/// <code>\n/// // String validation\n/// rule.Validate(\"\", \"Name\") // Returns \"Name must not be empty\"\n/// rule.Validate(\"   \", \"Name\") // Returns \"Name must not be empty\" (whitespace)\n/// rule.Validate(\"Value\", \"Name\") // Returns null (valid)\n/// \n/// // GUID validation\n/// rule.Validate(Guid.Empty, \"ID\") // Returns \"ID must not be empty\"\n/// rule.Validate(Guid.NewGuid(), \"ID\") // Returns null (valid)\n/// \n/// // Collection validation\n/// rule.Validate(new List&lt;string&gt;(), \"Items\") // Returns \"Items must not be empty\"\n/// rule.Validate(new[] { \"item\" }, \"Items\") // Returns null (valid)\n/// </code>\n/// </example>\npublic sealed class NotEmptyRule<T> : IRule<T>
{
    #region Public Methods

    /// <inheritdoc />\n    public string? Validate(T value, string displayName) =>
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