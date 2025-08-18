using System.Text.Json.Serialization;

namespace ShadowrunGM.UI.Application.Common.Results;

[JsonSerializable(typeof(ValidationProblemResponse))]
internal sealed partial class ValidationProblemJsonSerializerContext : JsonSerializerContext
{ }