namespace ShadowrunGM.UI.Application.Common.Results;

public sealed class GuidPropertyValidator<T> : PropertyValidator<T, Guid?, GuidPropertyValidator<T>>
{
    #region Internal Constructors

    internal GuidPropertyValidator(ValidationBuilder<T> builder, string propertyName, string displayName, Guid? value)
        : base(builder, propertyName, displayName, value)
    {
    }

    #endregion Internal Constructors
}