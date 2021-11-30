namespace SberGames.Utils.DataDownloader.Configuration
{
    public readonly struct DataDownloadAdditionalParameters
    {
        public readonly DataDownloadTimeouts? TimeoutsOverwrites;
        public readonly DataDownloadRangesInfo? RangesInfo;
        public readonly DataDownloadRequestInfo? RequestInfo;

        public DataDownloadAdditionalParameters(
            in DataDownloadTimeouts? timeoutsOverwrites = null,
            in DataDownloadRangesInfo? rangesInfo = null,
            in DataDownloadRequestInfo? requestInfo = null)
        {
            TimeoutsOverwrites = timeoutsOverwrites;
            RangesInfo = rangesInfo;
            RequestInfo = requestInfo;
        }
    }
}