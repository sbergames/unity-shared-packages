namespace SberGames.Utils.DataDownloader.DefaultDownloaders.FixedBuffer
{
    using System.Threading;
    using Configuration;

    public readonly struct FixedBufferDataDownloadParameters
    {
        public readonly string Url;
        public readonly CancellationToken CancellationToken;
        public readonly DataDownloadAdditionalParameters? AdditionalParameters;

        public FixedBufferDataDownloadParameters(
            string url,
            CancellationToken cancellationToken,
            in DataDownloadAdditionalParameters additionalParameters)
        {
            Url = url;
            CancellationToken = cancellationToken;
            AdditionalParameters = additionalParameters;
        }

        public FixedBufferDataDownloadParameters(
            string url,
            CancellationToken cancellationToken)
        {
            Url = url;
            CancellationToken = cancellationToken;
            AdditionalParameters = default;
        }
    }
}