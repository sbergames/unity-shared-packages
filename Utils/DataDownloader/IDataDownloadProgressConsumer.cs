namespace SberGames.Utils.DataDownloader
{
    using DefaultProgressConsumers;

    public interface IDataDownloadProgressConsumer
    {
        void InitializeWithTotalBytesCount(long totalBytes);

        void InitializeWithoutTotalBytesCount();


        void ReportDownloadedBytes(DownloadedBytes downloadedBytes);
    }
}