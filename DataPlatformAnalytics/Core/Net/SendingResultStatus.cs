using System;

namespace SberGames.DataPlatform.Core.Net
{
    [Flags]
    public enum SendingResultStatus
    {
        None = 0,
        IsSuccess = 1 << 0,
        IsTimeout = 1 << 1,
        IsCancelled = 1 << 2,
        IsServerError = 1 << 3,
        IsNetworkError = 1 << 4,
        IsInternalLogicError = 1 << 5,
    }
}