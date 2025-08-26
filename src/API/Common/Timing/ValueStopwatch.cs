namespace ShadowrunGM.API.Common.Timing;

internal readonly struct ValueStopwatch
{
    #region Private Members

    private readonly long _start;

    #endregion Private Members

    #region Private Constructors

    private ValueStopwatch(long start) => _start = start;

    #endregion Private Constructors

    #region Public Methods

    public static ValueStopwatch StartNew()
        => new(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

    #endregion Public Methods

    #region Public Properties

    public long ElapsedMilliseconds
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _start;

    #endregion Public Properties
}