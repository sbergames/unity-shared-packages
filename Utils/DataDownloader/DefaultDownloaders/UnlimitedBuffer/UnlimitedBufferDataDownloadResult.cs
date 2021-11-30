namespace SberGames.Utils.DataDownloader.DefaultDownloaders.UnlimitedBuffer
{
    using System.Buffers;
    using Result;

    public readonly struct UnlimitedBufferDataDownloadResult
    {
        public readonly DataDownloadResult Result;
        public readonly (IMemoryOwner<byte> memoryOwner, int bytesInMemory)? Data;

        public UnlimitedBufferDataDownloadResult(DataDownloadResult result, (IMemoryOwner<byte> memoryOwner, int bytesInMemory)? data)
        {
            Result = result;
            Data = data;
        }
    }
}