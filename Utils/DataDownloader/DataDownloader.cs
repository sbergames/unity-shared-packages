namespace SberGames.Utils.DataDownloader
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using DefaultProgressConsumers;
    using Exceptions;
    using Internal;
    using Result;

    public sealed class DataDownloader
    {
        private static readonly DataDownloadTimeouts DefaultTimeouts = new DataDownloadTimeouts(30_000, 5_000, 5_000);
        private readonly SimplePool<DownloadBytesCountKeeper> countKeeperPool = new SimplePool<DownloadBytesCountKeeper>(1);

        private readonly int dataChunkSizeBytes;
        private readonly DataDownloadTimeouts defaultTimeouts;
        private readonly HttpClient httpClient;

        public DataDownloader(HttpClient httpClient, int dataChunkSizeBytes, DataDownloadTimeouts? defaultTimeouts = null)
        {
            this.httpClient = httpClient;
            this.dataChunkSizeBytes = dataChunkSizeBytes;
            this.defaultTimeouts = defaultTimeouts ?? DefaultTimeouts;
        }

        public async Task<DataDownloadResult> Download(DataDownloadParameters downloadParameters)
        {
            return await Download(downloadParameters, new DataDownloadAdditionalParameters());
        }

        public async Task<DataDownloadResult> Download(DataDownloadParameters downloadParameters, DataDownloadAdditionalParameters additionalParameters)
        {
            HttpStatusCode? statusCode = default;
            CancellationTokenSource? overallTimeoutCancellationTokenSource = null;
            DataDownloadTimeouts timeouts = additionalParameters.TimeoutsOverwrites ?? defaultTimeouts;
            DownloadBytesCountKeeper countKeeper = countKeeperPool.Get();
            countKeeper.Reset();

            using CancellationTokenSource headersTimeoutCancellationTokenSource = new CancellationTokenSource(timeouts.NoHeadersTimeoutMs);

            try
            {
                using CancellationTokenSource getHeadersCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    downloadParameters.CancellationToken,
                    headersTimeoutCancellationTokenSource.Token);

                using HttpRequestMessage requestMessage = CreateRequestMessage(downloadParameters, additionalParameters);

                using HttpResponseMessage httpResponse = await httpClient.SendAsync(
                    requestMessage,
                    HttpCompletionOption.ResponseHeadersRead,
                    getHeadersCancellationTokenSource.Token).ConfigureAwait(false);
                
                headersTimeoutCancellationTokenSource.CancelAfter(-1);

                statusCode = httpResponse.StatusCode;

                if (TryFastExit(additionalParameters, httpResponse, out DataDownloadResult fastResult))
                {
                    return fastResult;
                }

                long? contentLength = httpResponse.Content.Headers.ContentLength;

                Stream readStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
                SetTimeoutToNetworkStream(readStream, timeouts.NoDataTimeoutMs);

                if (!readStream.CanRead)
                {
                    return new DataDownloadResult((int)statusCode, "Bad stream that cannot be read.", DownloadResultStatus.IsInternalLogicError, 0);
                }

                using (overallTimeoutCancellationTokenSource = new CancellationTokenSource(timeouts.OverallTimeoutMs))
                {
                    using CancellationTokenSource combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(downloadParameters.CancellationToken, overallTimeoutCancellationTokenSource.Token);

                    IDataDownloadProgressConsumer progressConsumer = downloadParameters.ProgressConsumer;

                    InitializeProgressConsumer(contentLength, progressConsumer);

                    await FeedAllBytesToProgressConsumer(
                        readStream,
                        timeouts.NoDataTimeoutMs,
                        progressConsumer,
                        countKeeper,
                        combinedCancellationTokenSource.Token).ConfigureAwait(false);

                    return new DataDownloadResult((int)statusCode, countKeeper.BytesDownloaded);
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                return new DataDownloadResult((int?)statusCode, httpRequestException.ToString(), DownloadResultStatus.IsNetworkError, countKeeper.BytesDownloaded);
            }
            catch (ProgressConsumerException progressConsumerException)
            {
                return new DataDownloadResult((int?)statusCode, progressConsumerException.ToString(), DownloadResultStatus.IsInternalLogicError, countKeeper.BytesDownloaded);
            }
            catch (TimeoutException timeoutException)
            {
                return new DataDownloadResult((int?)statusCode, $"No data read in {timeouts.NoDataTimeoutMs} milliseconds. This treated as an error.\n" + timeoutException, DownloadResultStatus.IsTimeout, countKeeper.BytesDownloaded);
            }
            catch (OperationCanceledException)
            {
                if (overallTimeoutCancellationTokenSource is { IsCancellationRequested: true })
                {
                    return new DataDownloadResult((int?)statusCode, $"Overall download timeout reached: {timeouts.OverallTimeoutMs} milliseconds.", DownloadResultStatus.IsTimeout, countKeeper.BytesDownloaded);
                }
                else if (headersTimeoutCancellationTokenSource.IsCancellationRequested)
                {
                    return new DataDownloadResult((int?)statusCode, $"Headers download timeout: {timeouts.NoDataTimeoutMs} milliseconds.", DownloadResultStatus.IsTimeout, countKeeper.BytesDownloaded);
                }
                else
                {
                    return new DataDownloadResult((int?)statusCode, "Operation cancelled from user code.", DownloadResultStatus.IsCancelled, countKeeper.BytesDownloaded);
                }
            }
            finally
            {
                countKeeperPool.Return(countKeeper);
            }
        }

        private void SetTimeoutToNetworkStream(Stream readStream, int timeoutMs)
        {
            if (readStream.CanTimeout)
            {
                readStream.ReadTimeout = timeoutMs;
            }
        }

        private async Task FeedAllBytesToProgressConsumer(
            Stream readStream,
            int noDataTimeoutMs,
            IDataDownloadProgressConsumer progressConsumer,
            DownloadBytesCountKeeper totalBytesReadKeeper,
            CancellationToken cancellationToken)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(dataChunkSizeBytes);

            using CancellationTokenSource noDataTimeoutCancellationTokenSource = new CancellationTokenSource(noDataTimeoutMs);
            using CancellationTokenSource combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(noDataTimeoutCancellationTokenSource.Token, cancellationToken);

            try
            {
                int bytesRead;
                int offset = 0;

                do
                {
                    noDataTimeoutCancellationTokenSource.CancelAfter(noDataTimeoutMs);

                    try
                    {
                        bytesRead = await readStream.ReadAsync(buffer, 0, dataChunkSizeBytes, combinedCancellationTokenSource.Token).ConfigureAwait(false);
                    }
                    catch (WebException webException)
                    {
                        throw new TimeoutException("Data stream read timeout.", webException);
                    }
                    catch (OperationCanceledException cancelledException)
                    {
                        if (noDataTimeoutCancellationTokenSource.IsCancellationRequested)
                        {
                            throw new TimeoutException("Data stream read timeout.", cancelledException);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    totalBytesReadKeeper.Add(bytesRead);

                    progressConsumer.ReportDownloadedBytes(
                        new DownloadedBytes(
                            new Span<byte>(buffer, 0, bytesRead),
                            offset));

                    offset += bytesRead;
                } while (bytesRead != 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private bool TryFastExit(in DataDownloadAdditionalParameters additionalParameters, HttpResponseMessage httpResponse, out DataDownloadResult dataDownloadResult)
        {
            HttpStatusCode statusCode = httpResponse.StatusCode;

            if (!httpResponse.IsSuccessStatusCode)
            {
                dataDownloadResult = new DataDownloadResult((int)statusCode, "Not successful response statusCode.", DownloadResultStatus.IsServerError, 0);

                return true;
            }

            DataDownloadRangesInfo? rangesInfo = additionalParameters.RangesInfo;

            if (rangesInfo is { FailIfRangesNotSatisfied: true } && statusCode != HttpStatusCode.PartialContent)
            {
                dataDownloadResult = new DataDownloadResult((int)statusCode, "Server does not respect ranges in request.", DownloadResultStatus.IsServerError, 0);

                return true;
            }

            dataDownloadResult = default;

            return false;
        }

        private HttpRequestMessage CreateRequestMessage(in DataDownloadParameters parameters, in DataDownloadAdditionalParameters additionalParameters)
        {
            HttpRequestMessage requestMessage;

            if (additionalParameters.RequestInfo.HasValue)
            {
                requestMessage = new HttpRequestMessage(additionalParameters.RequestInfo.Value.Method, parameters.Url);

                HttpContent? requestContent = additionalParameters.RequestInfo.Value.Content;

                if (requestContent != null)
                {
                    requestMessage.Content = requestContent;
                }
            }
            else
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, parameters.Url);
            }

            SetRangesHeaderIfNeeded(additionalParameters, requestMessage);

            return requestMessage;
        }

        private void InitializeProgressConsumer(long? contentLength, IDataDownloadProgressConsumer progressConsumer)
        {
            if (contentLength.HasValue)
            {
                progressConsumer.InitializeWithTotalBytesCount(contentLength.Value);
            }
            else
            {
                progressConsumer.InitializeWithoutTotalBytesCount();
            }
        }

        private void SetRangesHeaderIfNeeded(in DataDownloadAdditionalParameters additionalParameters, HttpRequestMessage requestMessage)
        {
            DataDownloadRangesInfo? rangesInfo = additionalParameters.RangesInfo;

            if (rangesInfo.HasValue)
            {
                requestMessage.Headers.Range = new RangeHeaderValue(rangesInfo.Value.StartByte, rangesInfo.Value.FinishByte);
            }
        }
    }
}