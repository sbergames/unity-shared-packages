namespace SberGames.Utils.DataDownloader.DefaultDownloaders.UnlimitedBuffer
{
    using System.Threading;
    using Configuration;

    public readonly struct UnlimitedBufferDataDownloadParameters
    {
        public readonly string Url;
        public readonly CancellationToken CancellationToken;
        public readonly DataDownloadAdditionalParameters? AdditionalParameters;

        public UnlimitedBufferDataDownloadParameters(
            string url,
            CancellationToken cancellationToken,
            in DataDownloadAdditionalParameters additionalParameters)
        {
            Url = url;
            CancellationToken = cancellationToken;
            AdditionalParameters = additionalParameters;
        }

        public UnlimitedBufferDataDownloadParameters(
            string url,
            CancellationToken cancellationToken)
        {
            Url = url;
            CancellationToken = cancellationToken;
            AdditionalParameters = default;
        }
    }
}