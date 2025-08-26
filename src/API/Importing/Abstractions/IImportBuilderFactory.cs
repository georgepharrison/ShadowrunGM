using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IImportBuilderFactory
{
    #region Public Methods

    IImportBuilder CreateFor(ImportWorkItem item, IServiceProvider services);

    #endregion Public Methods
}