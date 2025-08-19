using ShadowrunGM.API.Application.Common.Results.Rules;
using System.Numerics;

namespace ShadowrunGM.API.Application.Common.Results;

public sealed class NumericPropertyValidator<T, TNumeric> : PropertyValidator<T, TNumeric, NumericPropertyValidator<T, TNumeric>>
    where TNumeric : struct, INumber<TNumeric>
{
    #region Internal Constructors

    internal NumericPropertyValidator(ValidationBuilder<T> builder, string propertyName, string displayName, TNumeric value) : base(builder, propertyName, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods

    public NumericPropertyValidator<T, TNumeric> ExclusiveBetween(int from, int to) =>
        AddRule(new ExclusiveBetweenRule(from, to) as IRule<TNumeric>);

    public NumericPropertyValidator<T, TNumeric> GreaterThan(TNumeric valueToCompare) =>
        AddRule(new GreaterThanRule<TNumeric>(valueToCompare));

    public NumericPropertyValidator<T, TNumeric> GreaterThanOrEqualTo(TNumeric valueToCompare) =>
        AddRule(new GreaterThanOrEqualToRule<TNumeric>(valueToCompare));

    public NumericPropertyValidator<T, TNumeric> InclusiveBetween(int from, int to) =>
        AddRule(new InclusiveBetweenRule(from, to) as IRule<TNumeric>);

    public NumericPropertyValidator<T, TNumeric> LessThan(TNumeric valueToCompare) =>
        AddRule(new LessThanRule<TNumeric>(valueToCompare));

    public NumericPropertyValidator<T, TNumeric> LessThanOrEqualTo(TNumeric valueToCompare) =>
        AddRule(new LessThanOrEqualToRule<TNumeric>(valueToCompare));

    public NumericPropertyValidator<T, TNumeric> PrecisionScale(int precision, int scale) =>
        AddRule(new PrecisionScaleRule<TNumeric>(precision, scale));

    #endregion Public Methods
}