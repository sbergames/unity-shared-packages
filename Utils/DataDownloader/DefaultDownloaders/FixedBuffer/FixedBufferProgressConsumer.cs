namespace SberGames.Utils.DataDownloader.DefaultDownloaders.FixedBuffer
{
    using System;
    using DefaultProgressConsumers;
    using Exceptions;

    public sealed class FixedBufferProgressConsumer : IDataDownloadProgressConsumer
    {
        private readonly Memory<byte> buffer;
        private int bytesInBuffer;

        public FixedBufferProgressConsumer(Memory<byte> buffer)
        {
            this.buffer = buffer;
        }

        void IDataDownloadProgressConsumer.InitializeWithTotalBytesCount(long totalBytes)
        {
            if (buffer.Length < totalBytes)
            {
                throw new ProgressConsumerException($"Fixed buffer size {buffer.Length} bytes is less than data size going to be written in it {totalBytes} bytes");
            }

            bytesInBuffer = 0;
        }

        void IDataDownloadProgressConsumer.InitializeWithoutTotalBytesCount()
        {
            bytesInBuffer = 0;
        }

        void IDataDownloadProgressConsumer.ReportDownloadedBytes(DownloadedBytes downloadedBytes)
        {
            bytesInBuffer += downloadedBytes.Bytes.Length;

            if (bytesInBuffer > buffer.Length)
            {
                throw new ProgressConsumerException($"Fixed buffer size {buffer.Length} bytes is less than data size going to be written in it {bytesInBuffer} bytes");
            }

            downloadedBytes.Bytes.CopyTo(buffer.Span.Slice((int)downloadedBytes.OffsetInWholeData, downloadedBytes.Bytes.Length));
        }

        public Memory<byte> GetData()
        {
            return buffer.Slice(0, bytesInBuffer);
        }
    }
}