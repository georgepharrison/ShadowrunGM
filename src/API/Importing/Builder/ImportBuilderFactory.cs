using Microsoft.Extensions.AI;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Builder;

public sealed class ImportBuilderFactory : IImportBuilderFactory
{
    #region Public Methods

    public IImportBuilder CreateFor(ImportWorkItem item, IServiceProvider services)
    {
        string? extension = Path.GetExtension(item.SourceFilename)?.ToLowerInvariant();

        return extension switch
        {
            ".md" or ".markdown" or ".txt" => services.GetRequiredService<MarkdownImportBuilder>(),
            _ => services.GetRequiredService<MarkdownImportBuilder>()
        };
    }

    #endregion Public Methods
}
