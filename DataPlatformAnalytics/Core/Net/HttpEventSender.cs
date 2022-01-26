using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SberGames.DataPlatform.Core.Net
{
    public class HttpEventSender : IEventSender, IDisposable
    {
        private const string ApiKeyParam = "API_KEY";
        public const int TimeoutMs = 5000;
        
        private string apiKey;
        private string host;
        
        private HttpClient httpClient;

        public void Initialization(string _apiKey, string _host)
        {
            if (!_host.Contains(ApiKeyParam))
            {
                throw new Exception($"Incrorrect host, {ApiKeyParam} dosen't exist");
            }
            
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
            };
            
            httpClient = new HttpClient(handler, true);
            
            apiKey = _apiKey;
            host =  _host.Replace("{" + ApiKeyParam + "}", apiKey);
        }
        public async Task<SendingResult> Send(string data)
        {
            return await Send(CreateRequestMessage(data));
        }

        public async Task<SendingResult> Send(List<string> eventDatas)
        {
            return await Send(CreateRequestMessage(eventDatas));
        }

        private async Task<SendingResult> Send(HttpRequestMessage requestMessage)
        {
            HttpStatusCode? statusCode = default;

            using (CancellationTokenSource headersTimeoutCancellationTokenSource =
                new CancellationTokenSource(TimeoutMs))
            {
                try
                {
                    using (HttpResponseMessage httpResponse = await httpClient.SendAsync(
                        requestMessage,
                        HttpCompletionOption.ResponseHeadersRead,
                        headersTimeoutCancellationTokenSource.Token).ConfigureAwait(false))
                    {
                        headersTimeoutCancellationTokenSource.CancelAfter(-1);

                        statusCode = httpResponse.StatusCode;

                        if (TryFastExit(httpResponse, out SendingResult fastResult))
                        {
                            return fastResult;
                        }

                        return new SendingResult((int) statusCode);
                    }
                }
                catch (HttpRequestException httpRequestException)
                {
                    return new SendingResult((int?) statusCode, httpRequestException.ToString(),
                        SendingResultStatus.IsNetworkError);
                }
                catch (TimeoutException timeoutException)
                {
                    return new SendingResult((int?) statusCode,
                        $"Timeout exception. This treated as an error.\n" + timeoutException,
                        SendingResultStatus.IsTimeout);
                }
                catch (OperationCanceledException)
                {
                    if (headersTimeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new SendingResult((int?) statusCode, $"Cancellation requested.",
                            SendingResultStatus.IsTimeout);
                    }
                    else
                    {
                        return new SendingResult((int?) statusCode, "Operation cancelled from user code.",
                            SendingResultStatus.IsCancelled);
                    }
                }
                finally
                {
                }
            }
        }

        private bool TryFastExit(HttpResponseMessage httpResponse, out SendingResult dataDownloadResult)
        {
            HttpStatusCode statusCode = httpResponse.StatusCode;

            if (!httpResponse.IsSuccessStatusCode)
            {
                dataDownloadResult = new SendingResult((int)statusCode, "Not successful response statusCode.", SendingResultStatus.IsServerError);

                return true;
            }

            dataDownloadResult = default;

            return false;
        }
        
        private HttpRequestMessage CreateRequestMessage(string eventData)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, host);
            var data = "[" + eventData + "]";
            var httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            requestMessage.Content = httpContent;
            return requestMessage;
        }
        
        private HttpRequestMessage CreateRequestMessage(List<string> eventDatas)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, host);

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for(int i = 0; i < eventDatas.Count; i++)
            {
                sb.Append(eventDatas[i]);
                if (i < eventDatas.Count - 1)
                {
                    sb.Append(",");
                }
            }
            
            sb.Append("]");
            
            var httpContent = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            requestMessage.Content = httpContent;
            return requestMessage;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
