namespace SberGames.Utils.DataDownloader.DefaultDownloaders.FixedBuffer
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Configuration;
    using Result;

    public sealed class FixedBufferDataDownloader
    {
        private readonly DataDownloader dataDownloader;

        public FixedBufferDataDownloader(HttpClient httpClient, DataDownloadTimeouts? defaultTimeouts = null)
        {
            dataDownloader = new DataDownloader(httpClient, 1_000, defaultTimeouts);
        }

        public async Task<BufferDataDownloadResult> DownloadAsync(FixedBufferDataDownloadParameters parameters, int bufferSize)
        {
            return await DownloadAsync(parameters, new byte[bufferSize]);
        }

        public async Task<BufferDataDownloadResult> DownloadAsync(FixedBufferDataDownloadParameters parameters, Memory<byte> buffer)
        {
            FixedBufferProgressConsumer progressConsumer = new FixedBufferProgressConsumer(buffer);
            DataDownloadResult dataDownloadResult = await dataDownloader.Download(
                new DataDownloadParameters(
                    parameters.Url,
                    progressConsumer,
                    parameters.CancellationToken),
                parameters.AdditionalParameters ?? new DataDownloadAdditionalParameters());

            Memory<byte> downloadedData = progressConsumer.GetData();

            return new BufferDataDownloadResult(dataDownloadResult, downloadedData);
        }
    }
}