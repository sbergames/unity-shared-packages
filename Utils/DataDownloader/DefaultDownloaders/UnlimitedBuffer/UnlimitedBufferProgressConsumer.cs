namespace SberGames.Utils.DataDownloader.DefaultDownloaders.UnlimitedBuffer
{
    using System;
    using System.Buffers;
    using DefaultProgressConsumers;
    using Exceptions;

    public sealed class UnlimitedBufferProgressConsumer : IDataDownloadProgressConsumer
    {
        private int bytesInBuffer;

        private IMemoryOwner<byte>? memoryOwner;

        void IDataDownloadProgressConsumer.InitializeWithTotalBytesCount(long totalBytesToLoad)
        {
            bytesInBuffer = 0;
            memoryOwner = MemoryPool<byte>.Shared.Rent((int)totalBytesToLoad);
        }

        void IDataDownloadProgressConsumer.InitializeWithoutTotalBytesCount()
        {
            bytesInBuffer = 0;
            memoryOwner = MemoryPool<byte>.Shared.Rent();
        }

        void IDataDownloadProgressConsumer.ReportDownloadedBytes(DownloadedBytes downloadedBytes)
        {
            bytesInBuffer += downloadedBytes.Bytes.Length;

            if (memoryOwner == null)
            {
                throw new ProgressConsumerException("MemoryOwner is null. Should never be the case at this moment (downloaded bytes being reported).");
            }

            Memory<byte> bufferMemory = EnlargeBufferIfNeeded(ref memoryOwner, bytesInBuffer);

            downloadedBytes.Bytes.CopyTo(bufferMemory.Span.Slice((int)downloadedBytes.OffsetInWholeData, downloadedBytes.Bytes.Length));
        }

        public (IMemoryOwner<byte> memoryOwner, int bytesLoaded) TakeAwayData()
        {
            if (memoryOwner == null)
            {
                throw new ProgressConsumerException("MemoryOwner is null. Should never be the case at this moment (data is taken away from consumer).");
            }

            IMemoryOwner<byte>? tempMemoryOwner = memoryOwner;
            memoryOwner = null;

            return (tempMemoryOwner, bytesInBuffer);
        }

        private Memory<byte> EnlargeBufferIfNeeded(ref IMemoryOwner<byte> owner, int bytesNeeded)
        {
            Memory<byte> bufferMemory = owner.Memory;

            if (bufferMemory.Length < bytesNeeded)
            {
                owner.Dispose();

                int newBufferSize = (int)(bytesNeeded * 1.5f);
                owner = MemoryPool<byte>.Shared.Rent(newBufferSize);
                bufferMemory = owner.Memory;
            }

            return bufferMemory;
        }
    }
}