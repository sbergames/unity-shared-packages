namespace SberGames.Utils.DataDownloader.Configuration
{
    public readonly struct DataDownloadRangesInfo
    {
        public readonly long StartByte;
        public readonly long? FinishByte;
        public readonly bool FailIfRangesNotSatisfied;

        public DataDownloadRangesInfo(long startByte, bool failIfRangesNotSatisfied = true)
        {
            StartByte = startByte;
            FinishByte = null;
            FailIfRangesNotSatisfied = failIfRangesNotSatisfied;
        }

        public DataDownloadRangesInfo(long startByte, long finishByte, bool failIfRangesNotSatisfied = true)
        {
            StartByte = startByte;
            FinishByte = finishByte;
            FailIfRangesNotSatisfied = failIfRangesNotSatisfied;
        }
    }
}