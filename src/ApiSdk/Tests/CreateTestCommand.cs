using FlowRight.Cqrs.Http;

namespace ShadowrunGM.ApiSdk.Tests;

public sealed partial record CreateTestCommand(string Title) : ICommand
{
  public string GetApiEndpoint() =>
    "test";
}
