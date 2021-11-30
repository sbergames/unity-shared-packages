namespace SberGames.Utils.DataDownloader.Configuration
{
    using System.Threading;

    public readonly struct DataDownloadParameters
    {
        public readonly string Url;
        public readonly IDataDownloadProgressConsumer ProgressConsumer;
        public readonly CancellationToken CancellationToken;

        public DataDownloadParameters(
            string url,
            IDataDownloadProgressConsumer progressConsumer,
            CancellationToken cancellationToken)
        {
            Url = url;
            ProgressConsumer = progressConsumer;
            CancellationToken = cancellationToken;
        }
    }
}