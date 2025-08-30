using ShadowrunGM.ApiSdk.Common.Results;

namespace ShadowrunGM.Domain.Tests.TestHelpers;

/// <summary>
/// Extension methods for Result types to provide test-friendly validation properties.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Gets a value indicating whether the result failed due to validation errors.
    /// </summary>
    public static bool IsValidationException<T>(this Result<T> result) =>
        result.FailureType == ResultFailureType.Validation;

    /// <summary>
    /// Gets a value indicating whether the result failed due to validation errors.
    /// </summary>
    public static bool IsValidationException(this Result result) =>
        result.FailureType == ResultFailureType.Validation;

    /// <summary>
    /// Gets the validation errors dictionary for convenience in tests.
    /// </summary>
    public static Dictionary<string, string[]> ValidationErrors<T>(this Result<T> result) =>
        result.Failures.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    /// <summary>
    /// Gets the validation errors dictionary for convenience in tests.
    /// </summary>
    public static Dictionary<string, string[]> ValidationErrors(this Result result) =>
        result.Failures.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}