namespace SberGames.Utils.DataDownloader.Internal
{
    internal sealed class DownloadBytesCountKeeper
    {
        public long BytesDownloaded { get; private set; }

        public void Reset()
        {
            BytesDownloaded = 0;
        }

        public void Add(int bytesRead)
        {
            BytesDownloaded += bytesRead;
        }
    }
}