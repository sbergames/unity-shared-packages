namespace SberGames.Utils.DataDownloader.DefaultProgressConsumers
{
    using System;

    public sealed class SimpleProgressConsumer : IDataDownloadProgressConsumer
    {
        private readonly Action<float> progressionCallback;
        private long currentBytesCount;

        private long? totalBytesCount;

        public SimpleProgressConsumer(Action<float> progressionCallback)
        {
            this.progressionCallback = progressionCallback;
        }

        public void InitializeWithTotalBytesCount(long totalBytes)
        {
            totalBytesCount = totalBytes;
            currentBytesCount = 0;
        }

        public void InitializeWithoutTotalBytesCount()
        {
            totalBytesCount = default;
            currentBytesCount = 0;
        }

        public void ReportDownloadedBytes(DownloadedBytes downloadedBytes)
        {
            if (!totalBytesCount.HasValue)
            {
                progressionCallback.Invoke(0.0f);

                return;
            }

            currentBytesCount = downloadedBytes.OffsetInWholeData + downloadedBytes.Bytes.Length;
            double progress = (double)currentBytesCount / totalBytesCount.Value;

            progressionCallback.Invoke((float)progress);
        }
    }
}