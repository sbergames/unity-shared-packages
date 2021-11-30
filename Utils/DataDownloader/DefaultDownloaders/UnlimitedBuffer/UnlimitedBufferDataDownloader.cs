namespace SberGames.Utils.DataDownloader.DefaultDownloaders.UnlimitedBuffer
{
    using System.Buffers;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Configuration;
    using Result;

    public sealed class UnlimitedBufferDataDownloader
    {
        private readonly DataDownloader dataDownloader;

        public UnlimitedBufferDataDownloader(HttpClient httpClient, DataDownloadTimeouts? defaultTimeouts = null)
        {
            dataDownloader = new DataDownloader(httpClient, 1_000, defaultTimeouts);
        }

        public async Task<UnlimitedBufferDataDownloadResult> DownloadAsync(UnlimitedBufferDataDownloadParameters parameters)
        {
            var progressConsumer = new UnlimitedBufferProgressConsumer();
            DataDownloadResult dataDownloadResult = await dataDownloader.Download(
                new DataDownloadParameters(
                    parameters.Url,
                    progressConsumer,
                    parameters.CancellationToken),
                parameters.AdditionalParameters ?? new DataDownloadAdditionalParameters());

            bool isSomethingDownloaded = dataDownloadResult.DownloadedBytesCount > 0;
            if (isSomethingDownloaded)
            {
                (IMemoryOwner<byte> memoryOwner, int bytesLoaded) downloadedData = progressConsumer.TakeAwayData();
                return new UnlimitedBufferDataDownloadResult(dataDownloadResult, downloadedData);
            }
            
            return new UnlimitedBufferDataDownloadResult(dataDownloadResult, null);
        }
    }
}