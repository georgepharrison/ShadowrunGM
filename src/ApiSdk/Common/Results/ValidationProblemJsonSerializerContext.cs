using System.Text.Json.Serialization;

namespace ShadowrunGM.ApiSdk.Common.Results;

[JsonSerializable(typeof(ValidationProblemResponse))]
internal sealed partial class ValidationProblemJsonSerializerContext : JsonSerializerContext
{ }