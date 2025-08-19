using ShadowrunGM.API.Application.Common.Results.Rules;
using System.Linq.Expressions;

namespace ShadowrunGM.API.Application.Common.Results;

public abstract class PropertyValidator<T, TProp, TRule>
    where TRule : PropertyValidator<T, TProp, TRule>
{
    #region Private Members

    private readonly ValidationBuilder<T> _builder;
    private readonly string _displayName;
    private readonly List<(IRule<TProp>? rule, string? customMessage, Func<TProp, bool>? condition)> _pendingRules = [];
    private readonly string _propertyName;
    private readonly TProp _value;

    #endregion Private Members

    #region Internal Constructors

    internal PropertyValidator(ValidationBuilder<T> builder, string propertyName, string displayName, TProp value)
    {
        _builder = builder;
        _propertyName = propertyName;
        _displayName = displayName;
        _value = value;
    }

    #endregion Internal Constructors

    #region Public Methods

    public Result<T> Build(Func<T> factory)
    {
        ApplyPendingRules();
        return _builder.Build(factory);
    }

    public TRule Empty()
    {
        _pendingRules.Add((new EmptyRule<TProp>(), null, null));
        return (TRule)this;
    }

    public TRule Equal(TProp comparisonValue, IEqualityComparer<TProp>? comparer = null)
    {
        _pendingRules.Add((new EqualRule<TProp>(comparisonValue, comparer), null, null));
        return (TRule)this;
    }

    public TRule Must(Func<TProp, bool> condition, string errorMessage)
    {
        _pendingRules.Add((new MustRule<TProp>(condition, errorMessage), null, null));
        return (TRule)this;
    }

    public TRule NotEmpty()
    {
        _pendingRules.Add((new NotEmptyRule<TProp>(), null, null));
        return (TRule)this;
    }

    public TRule NotEqual(TProp comparisonValue, IEqualityComparer<TProp>? comparer = null)
    {
        _pendingRules.Add((new NotEqualRule<TProp>(comparisonValue, comparer), null, null));
        return (TRule)this;
    }

    public TRule Notnull()
    {
        _pendingRules.Add((new NotNullRule<TProp>(), null, null));
        return (TRule)this;
    }

    public TRule Null()
    {
        _pendingRules.Add((new NullRule<TProp>(), null, null));
        return (TRule)this;
    }

    public StringPropertyValidator<T> RuleFor(Expression<Func<T, string>> propertySelector, string value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public GuidPropertyValidator<T> RuleFor(Expression<Func<T, Guid?>> propertySelector, Guid? value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, int> RuleFor(Expression<Func<T, int>> propertySelector, int value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, long> RuleFor(Expression<Func<T, long>> propertySelector, long value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, decimal> RuleFor(Expression<Func<T, decimal>> propertySelector, decimal value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, double> RuleFor(Expression<Func<T, double>> propertySelector, double value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, float> RuleFor(Expression<Func<T, float>> propertySelector, float value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, short> RuleFor(Expression<Func<T, short>> propertySelector, short value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public EnumerablePropertyValidator<T, TItem> RuleFor<TItem>(Expression<Func<T, IEnumerable<TItem>>> propertySelector, IEnumerable<TItem> value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public TRule Unless(Func<TProp, bool> condition) =>
        When(value => !condition(value));

    public TRule When(Func<TProp, bool> condition)
    {
        UpdateLastRuleCondition(condition);
        return (TRule)this;
    }

    public TRule WithMessage(string customMessage)
    {
        UpdateLastValidationMessage(customMessage);
        return (TRule)this;
    }

    #endregion Public Methods

    #region Protected Methods

    protected TRule AddRule(IRule<TProp>? rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        _pendingRules.Add((rule, null, null));

        return (TRule)this;
    }

    protected void UpdateLastRuleCondition(Func<TProp, bool>? condition)
    {
        if (_pendingRules.Count > 0)
        {
            (IRule<TProp>? rule, string? message, _) = _pendingRules[^1];
            _pendingRules[^1] = (rule, message, condition);
        }
    }

    protected void UpdateLastValidationMessage(string? customMessage)
    {
        if (_pendingRules.Count > 0)
        {
            (IRule<TProp>? rule, _, Func<TProp, bool>? condition) = _pendingRules[^1];
            _pendingRules[^1] = (rule, customMessage, condition);
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private void ApplyPendingRules()
    {
        foreach ((IRule<TProp>? rule, string? customMessage, Func<TProp, bool>? condition) in _pendingRules)
        {
            if (rule is not null)
            {
                if (condition is not null && !condition(_value))
                {
                    continue;
                }

                string? error = rule.Validate(_value, _displayName);

                if (error is not null)
                {
                    string finalMessage = customMessage ?? error;

                    _builder.AddError(_propertyName, finalMessage);
                }
            }
        }
        _pendingRules.Clear();
    }

    #endregion Private Methods
}