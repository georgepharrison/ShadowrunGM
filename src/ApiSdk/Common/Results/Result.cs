using System.Security;
using System.Text.Json.Serialization;

namespace ShadowrunGM.ApiSdk.Common.Results;

public partial class Result : IResult
{
    #region Private Constructors

    [JsonConstructor]
    private Result()
    { }

    private Result(ResultType resultType = ResultType.Success) =>
        ResultType = resultType;

    private Result(string error, ResultType resultType)
    {
        Error = error;
        ResultType = resultType;
    }

    private Result(string key, string error)
    {
        FailureType = ResultFailureType.Validation;
        ResultType = ResultType.Error;
        Failures.Add(key, [error]);
        Error = GetValidationError(Failures);
    }

    private Result(SecurityException securityException)
    {
        FailureType = ResultFailureType.Security;
        ResultType = ResultType.Error;
        Error = securityException.Message;
    }

    private Result(OperationCanceledException operationCanceledException)
    {
        FailureType = ResultFailureType.OperationCanceled;
        ResultType = ResultType.Warning;
        Error = operationCanceledException.Message;
    }

    private Result(IDictionary<string, string[]> errors)
    {
        FailureType = ResultFailureType.Validation;
        ResultType = ResultType.Error;
        Failures = errors;
        Error = GetValidationError(errors);
    }

    #endregion Private Constructors

    #region Internal Methods

    internal static string GetValidationError(IDictionary<string, string[]> errors)
    {
        List<string> errorMessages = [$"One or more validation errors occurred.{Environment.NewLine}"];

        foreach (KeyValuePair<string, string[]> error in errors)
        {
            errorMessages.Add($"{error.Key}");

            foreach (string errorMessage in error.Value)
            {
                errorMessages.Add($"  - {errorMessage}");
            }
        }

        return string.Join(Environment.NewLine, errorMessages);
    }

    #endregion Internal Methods

    #region Public Properties

    [JsonInclude]
    public string Error { get; private set; } = string.Empty;

    [JsonInclude]
    public IDictionary<string, string[]> Failures { get; private set; } = new Dictionary<string, string[]>();

    [JsonInclude]
    public ResultFailureType FailureType { get; private set; } = ResultFailureType.None;

    public bool IsFailure =>
        !string.IsNullOrEmpty(Error);

    public bool IsSuccess =>
        !IsFailure;

    [JsonInclude]
    public ResultType ResultType { get; private set; } = ResultType.Information;

    #endregion Public Properties
}