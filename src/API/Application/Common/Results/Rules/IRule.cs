namespace ShadowrunGM.UI.Application.Common.Results.Rules;

public interface IRule<in T>
{
    #region Public Methods

    string? Validate(T value, string displayName);

    #endregion Public Methods
}