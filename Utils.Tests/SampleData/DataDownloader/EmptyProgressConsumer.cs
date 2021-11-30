namespace Utils.Tests.SampleData.DataDownloader
{
    using SberGames.Utils.DataDownloader;
    using SberGames.Utils.DataDownloader.DefaultProgressConsumers;

    public sealed class EmptyProgressConsumer : IDataDownloadProgressConsumer
    {
        public void InitializeWithTotalBytesCount(long totalBytes)
        {
        }

        public void InitializeWithoutTotalBytesCount()
        {
        }

        public void ReportDownloadedBytes(DownloadedBytes downloadedBytes)
        {
        }
    }
}