namespace SberGames.Utils.DataDownloader.DefaultProgressConsumers
{
    using System;
    using System.Threading;

    public sealed class ContextBindedProgressConsumer : IDataDownloadProgressConsumer, IDisposable
    {
        private readonly SynchronizationContext context;
        private readonly Action<float> progressionCallback;
        private long currentBytesCount;

        private volatile bool isDisposed;

        private long? totalBytesCount;

        public ContextBindedProgressConsumer(Action<float> progressionCallback, SynchronizationContext context)
        {
            this.progressionCallback = progressionCallback;
            this.context = context;
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
                context.Post(InvokeCallback, 0.0f);

                return;
            }

            currentBytesCount = downloadedBytes.OffsetInWholeData + downloadedBytes.Bytes.Length;
            double progress = (double)currentBytesCount / totalBytesCount.Value;

            context.Post(InvokeCallback, (float)progress);
        }

        public void Dispose()
        {
            isDisposed = true;
        }

        private void InvokeCallback(object state)
        {
            if (isDisposed)
            {
                return;
            }

            progressionCallback.Invoke((float)state);
        }
    }
}