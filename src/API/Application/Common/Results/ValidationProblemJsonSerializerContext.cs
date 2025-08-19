using System.Text.Json.Serialization;

namespace ShadowrunGM.API.Application.Common.Results;

[JsonSerializable(typeof(ValidationProblemResponse))]
internal sealed partial class ValidationProblemJsonSerializerContext : JsonSerializerContext
{ }