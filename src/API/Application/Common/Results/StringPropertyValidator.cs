using ShadowrunGM.API.Application.Common.Results.Rules;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Application.Common.Results;

public sealed class StringPropertyValidator<T> : PropertyValidator<T, string, StringPropertyValidator<T>>
{
    #region Internal Constructors

    internal StringPropertyValidator(ValidationBuilder<T> builder, string displayName, string value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods

    public StringPropertyValidator<T> EmailAddress() =>
        AddRule(new EmailRule());

    public StringPropertyValidator<T> ExactLength(int length) =>
        AddRule(new ExactLengthRule(length));

    public StringPropertyValidator<T> Length(int minLength, int maxLength) =>
        AddRule(new LengthRule(minLength, maxLength));

    public StringPropertyValidator<T> Matches(string pattern, RegexOptions options = RegexOptions.None) =>
        AddRule(new MatchesRule(pattern, options));

    public StringPropertyValidator<T> MaximumLength(int max) =>
        AddRule(new MaxLengthRule(max));

    public StringPropertyValidator<T> MinimumLength(int min) =>
        AddRule(new MinLengthRule(min));

    #endregion Public Methods
}