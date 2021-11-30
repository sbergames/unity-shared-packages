namespace SberGames.Utils.DataDownloader.DefaultDownloaders.FixedBuffer
{
    using System;
    using Result;

    public readonly struct BufferDataDownloadResult
    {
        public readonly DataDownloadResult Result;
        public readonly Memory<byte>? Buffer;

        public BufferDataDownloadResult(DataDownloadResult result, Memory<byte>? buffer)
        {
            Result = result;
            Buffer = buffer;
        }
    }
}