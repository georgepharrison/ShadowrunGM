using System.Linq.Expressions;
using System.Numerics;

namespace ShadowrunGM.API.Application.Common.Results;

public class ValidationBuilder<T>
{
    #region Private Members

    private readonly Dictionary<string, List<string>> _errors = [];

    #endregion Private Members

    #region Public Methods

    public Result<T> Build(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return HasErrors
            ? Result.Failure<T>(GetErrors())
            : Result.Success(factory());
    }

    public Dictionary<string, string[]> GetErrors() =>
        _errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());

    public GuidPropertyValidator<T> RuleFor(Expression<Func<T, Guid?>> propertySelector, Guid? value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, name, display, val) => new GuidPropertyValidator<T>(builder, name, display, val));

    public StringPropertyValidator<T> RuleFor(Expression<Func<T, string>> propertySelector, string value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, name, display, val) => new StringPropertyValidator<T>(builder, name, display, val));

    public NumericPropertyValidator<T, int> RuleFor(Expression<Func<T, int>> propertySelector, int value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, long> RuleFor(Expression<Func<T, long>> propertySelector, long value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, decimal> RuleFor(Expression<Func<T, decimal>> propertySelector, decimal value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, double> RuleFor(Expression<Func<T, double>> propertySelector, double value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, float> RuleFor(Expression<Func<T, float>> propertySelector, float value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, short> RuleFor(Expression<Func<T, short>> propertySelector, short value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public EnumerablePropertyValidator<T, TItem> RuleFor<TItem>(Expression<Func<T, IEnumerable<TItem>>> propertySelector, IEnumerable<TItem> value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, name, display, val) => new EnumerablePropertyValidator<T, TItem>(builder, name, display, val));

    #endregion Public Methods

    #region Internal Methods

    internal void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out List<string>? value))
        {
            value = [];

            _errors[propertyName] = value;
        }

        value.Add(error);
    }

    #endregion Internal Methods

    #region Private Methods

    private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> propertySelector) =>
        propertySelector?.Body is MemberExpression member
            ? member.Member.Name
            : throw new ArgumentException("Expression must be a property selector");

    private NumericPropertyValidator<T, TProp> CreateNumericValidator<TProp>(Expression<Func<T, TProp>> propertySelector, TProp value, string? displayName) where TProp : struct, INumber<TProp> =>
        CreateValidator(propertySelector, value, displayName, (builder, name, display, val) => new NumericPropertyValidator<T, TProp>(builder, name, display, val));

    private TValidator CreateValidator<TValidator, TProp>(
        Expression<Func<T, TProp>> propertySelector,
        TProp value,
        string? displayName,
        Func<ValidationBuilder<T>, string, string, TProp, TValidator> factory)
    {
        string propertyName = GetPropertyName(propertySelector);
        string friendlyName = displayName ?? propertyName;
        return factory(this, propertyName, friendlyName, value);
    }

    #endregion Private Methods

    #region Public Properties

    public bool HasErrors =>
        _errors.Count is not 0;

    #endregion Public Properties
}