using ShadowrunGM.API.Importing.Jobs;

namespace ShadowrunGM.API.Importing.Contracts;

public sealed record ImportProgress(ImportStep Step, int Percent, string Message = "");