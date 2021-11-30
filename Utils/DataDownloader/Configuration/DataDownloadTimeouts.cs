namespace SberGames.Utils.DataDownloader.Configuration
{
    public readonly struct DataDownloadTimeouts
    {
        public readonly int OverallTimeoutMs;
        public readonly int NoDataTimeoutMs;
        public readonly int NoHeadersTimeoutMs;

        public DataDownloadTimeouts(
            int overallTimeoutMs,
            int noDataTimeoutMs,
            int noHeadersTimeoutMs)
        {
            OverallTimeoutMs = overallTimeoutMs;
            NoDataTimeoutMs = noDataTimeoutMs;
            NoHeadersTimeoutMs = noHeadersTimeoutMs;
        }
    }
}