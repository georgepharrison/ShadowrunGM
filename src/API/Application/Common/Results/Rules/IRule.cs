namespace ShadowrunGM.API.Application.Common.Results.Rules;

/// <summary>
/// Defines a validation rule that can be applied to values of type T, providing error messages when validation fails.
/// </summary>
/// <typeparam name="T">The type of value this rule can validate.</typeparam>
public interface IRule<in T>
{
    #region Public Methods

    /// <summary>\n    /// Validates the specified value and returns an error message if validation fails.\n    /// </summary>\n    /// <param name=\"value\">The value to validate.</param>\n    /// <param name=\"displayName\">The display name for the property being validated, used in error messages.</param>\n    /// <returns>An error message if validation fails; otherwise, null indicating validation passed.</returns>\n    string? Validate(T value, string displayName);

    #endregion Public Methods
}