namespace SberGames.Utils.DataDownloader.DefaultProgressConsumers
{
    using System;

    public readonly ref struct DownloadedBytes
    {
        public readonly Span<byte> Bytes;
        public readonly long OffsetInWholeData;

        public DownloadedBytes(in Span<byte> bytes, long offsetInWholeData)
        {
            Bytes = bytes;
            OffsetInWholeData = offsetInWholeData;
        }
    }
}