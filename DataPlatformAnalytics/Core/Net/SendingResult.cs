
namespace SberGames.DataPlatform.Core.Net
{
    public readonly struct SendingResult
    {
        public readonly int? StatusCode;
        public readonly string? Error;

        private readonly SendingResultStatus resultStatus;

        public bool IsNone => resultStatus == SendingResultStatus.None;

        public bool IsSuccess => (resultStatus & SendingResultStatus.IsSuccess) == SendingResultStatus.IsSuccess;

        public bool IsTimeout => (resultStatus & SendingResultStatus.IsTimeout) == SendingResultStatus.IsTimeout;

        public bool IsCancelled => (resultStatus & SendingResultStatus.IsCancelled) == SendingResultStatus.IsCancelled;
        
        public bool IsNetworkError => (resultStatus & SendingResultStatus.IsNetworkError) == SendingResultStatus.IsNetworkError;
        
        public bool IsServerError => (resultStatus & SendingResultStatus.IsServerError) == SendingResultStatus.IsServerError;
        
        public bool IsInternalLogicError => (resultStatus & SendingResultStatus.IsInternalLogicError) == SendingResultStatus.IsInternalLogicError;

        public SendingResult(int statusCode)
        {
            resultStatus = SendingResultStatus.IsSuccess;
            StatusCode = statusCode;
            Error = default;
        }

        public SendingResult(int? statusCode, string? error, SendingResultStatus status)
        {
            resultStatus = status;
            StatusCode = statusCode;
            Error = error;
        }
    }
}
