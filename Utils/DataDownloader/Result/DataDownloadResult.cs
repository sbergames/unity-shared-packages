namespace SberGames.Utils.DataDownloader.Result
{
    public readonly struct DataDownloadResult
    {
        public readonly long DownloadedBytesCount;
        public readonly int? StatusCode;
        public readonly string? Error;

        private readonly DownloadResultStatus resultStatus;

        public bool IsNone => resultStatus == DownloadResultStatus.None;

        public bool IsSuccess => (resultStatus & DownloadResultStatus.IsSuccess) == DownloadResultStatus.IsSuccess;

        public bool IsTimeout => (resultStatus & DownloadResultStatus.IsTimeout) == DownloadResultStatus.IsTimeout;

        public bool IsCancelled => (resultStatus & DownloadResultStatus.IsCancelled) == DownloadResultStatus.IsCancelled;
        
        public bool IsNetworkError => (resultStatus & DownloadResultStatus.IsNetworkError) == DownloadResultStatus.IsNetworkError;
        
        public bool IsServerError => (resultStatus & DownloadResultStatus.IsServerError) == DownloadResultStatus.IsServerError;
        
        public bool IsInternalLogicError => (resultStatus & DownloadResultStatus.IsInternalLogicError) == DownloadResultStatus.IsInternalLogicError;

        public DataDownloadResult(int statusCode, long downloadedBytesCount)
        {
            resultStatus = DownloadResultStatus.IsSuccess;
            StatusCode = statusCode;
            DownloadedBytesCount = downloadedBytesCount;
            Error = default;
        }

        public DataDownloadResult(int? statusCode, string? error, DownloadResultStatus status, long downloadedBytesCount)
        {
            resultStatus = status;
            StatusCode = statusCode;
            DownloadedBytesCount = downloadedBytesCount;
            Error = error;
        }
    }
}