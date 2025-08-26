using ShadowrunGM.API.Importing.Contracts;
using System.Threading.Channels;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IImportQueue
{
    #region Public Methods

    ValueTask EnqueueAsync(ImportWorkItem item, CancellationToken cancellationToken = default);

    #endregion Public Methods

    #region Public Properties

    ChannelReader<ImportWorkItem> Reader { get; }

    #endregion Public Properties
}