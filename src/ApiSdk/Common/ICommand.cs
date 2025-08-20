namespace ShadowrunGM.ApiSdk.Common;

public interface ICommand : IRequest
{
}

public interface ICommand<TResult> : IRequest
{
}