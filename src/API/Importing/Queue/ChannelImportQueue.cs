using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using System.Threading.Channels;

namespace ShadowrunGM.API.Endpoints;

public sealed class ChannelImportQueue : IImportQueue
{
    #region Private Members

    private readonly Channel<ImportWorkItem> _channel;

    #endregion Private Members

    #region Public Constructors

    public ChannelImportQueue(int capacity = 200)
    {
        BoundedChannelOptions options = new(capacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<ImportWorkItem>(options);
    }

    #endregion Public Constructors

    #region Public Methods

    public ValueTask EnqueueAsync(ImportWorkItem item, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(item, cancellationToken);

    #endregion Public Methods

    #region Public Properties

    public ChannelReader<ImportWorkItem> Reader =>
        _channel.Reader;

    #endregion Public Properties
}
