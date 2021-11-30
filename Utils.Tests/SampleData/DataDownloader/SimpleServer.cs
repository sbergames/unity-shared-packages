namespace Utils.Tests.SampleData.DataDownloader
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using CompressionLevel = System.IO.Compression.CompressionLevel;

    internal class SimpleServer
    {
        private readonly HttpListener listener;

        private bool isListening;

        public SimpleServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:9999/");
        }

        public async void StartListen()
        {
            listener.Start();
            isListening = true;

            try
            {
                await Task.Run(async () => { await Listen(); });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Destroy()
        {
            isListening = false;
            listener.Abort();
        }

        private async Task Listen()
        {
            while (isListening)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HandleRequest(context);
            }
        }
        
        private async void HandleRequest(HttpListenerContext context)
        {
            try
            {
                await Task.Run(
                    async () =>
                    {
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;

                        string? requestType = request.QueryString.Get("requestType");
                        await AnswerByRequestType(requestType, response, request);
                    }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                // Exception thrown while handling one request cannot be the reason for whole server down.
            }
        }

        private async Task AnswerByRequestType(string? requestType, HttpListenerResponse response, HttpListenerRequest httpListenerRequest)
        {
            if (string.IsNullOrWhiteSpace(requestType))
            {
                await HandleEmptyRequest(response);

                return;
            }

            switch (requestType)
            {
                case "normal":
                    await HandleNormalResponse(response);

                    break;

                case "ranges":
                    await HandleRangesResponse(response, httpListenerRequest);

                    break;

                case "post":
                    await HandlePostResponse(response, httpListenerRequest);

                    break;

                case "normalGzipped":
                    await HandleNormalGzippedResponse(response);

                    break;

                case "bigObject":
                    await HandleBigObjectResponse(response);

                    break;

                case "bigObjectByChunks":
                    await HandleBigObjectByChunksResponse(response);

                    break;

                case "bigObjectByChunksWithUnexpectedStop":
                    await HandleBigObjectByChunksWithUnexpectedStopResponse(response);

                    break;

                case "bigObjectByChunksGZippedWithUnexpectedStop":
                    await HandleBigObjectGZippedByChunksWithUnexpectedStopResponse(response);

                    break;

                case "404":
                    await Handle404Response(response);

                    break;

                case "headersTimeout":
                    await HandleHeadersTimeoutResponse(response);

                    break;

                case "goodHeadersNoData":
                    await HandleGoodHeadersNoDataResponse(response);

                    break;
            }
        }

        private async Task HandleGoodHeadersNoDataResponse(HttpListenerResponse response)
        {
            response.ContentLength64 = 10;
            Stream output = response.OutputStream;
            await output.WriteAsync(Array.Empty<byte>(), 0, 0);
            await Task.Delay(1100);
            output.Close();
        }

        private async Task HandleHeadersTimeoutResponse(HttpListenerResponse response)
        {
            await Task.Delay(1100);
            response.ContentLength64 = 0;
            Stream output = response.OutputStream;
            await output.WriteAsync(Array.Empty<byte>(), 0, 0);
            output.Close();
        }

        private async Task Handle404Response(HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.ContentLength64 = 0;
            Stream output = response.OutputStream;
            await output.WriteAsync(Array.Empty<byte>(), 0, 0);
            output.Close();
        }

        private async Task HandleBigObjectResponse(HttpListenerResponse response)
        {
            byte[] buffer = new byte[60 * 1024 * 1024];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 1;
            }

            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            output.Close();
        }

        private async Task HandleBigObjectByChunksResponse(HttpListenerResponse response)
        {
            byte[] buffer = new byte[60 * 1024 * 1024];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 1;
            }

            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;

            for (int i = 0; i < 60; i++)
            {
                await output.WriteAsync(buffer, i * 1024 * 1024, 1024 * 1024);
                await Task.Delay(20);
            }

            output.Close();
        }

        private async Task HandleBigObjectByChunksWithUnexpectedStopResponse(HttpListenerResponse response)
        {
            byte[] buffer = new byte[60 * 1024 * 1024];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 1;
            }

            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;

            for (int i = 0; i < 30; i++)
            {
                await output.WriteAsync(buffer, i * 1024 * 1024, 1024 * 1024);
                await Task.Delay(20);
            }

            await Task.Delay(2000);

            output.Close();
        }

        private async Task HandleBigObjectGZippedByChunksWithUnexpectedStopResponse(HttpListenerResponse response)
        {
            byte[] buffer = new byte[60 * 1024 * 1024];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)i;
            }

            try
            {
                byte[] gZippedBuffer;
                {
                    using MemoryStream memoryStreamInput = new MemoryStream(buffer);
                    using MemoryStream memoryStreamOutput = new MemoryStream();
                    using GZipStream gZipStream = new GZipStream(memoryStreamOutput, CompressionLevel.Optimal, true);
                    await gZipStream.WriteAsync(buffer, 0, buffer.Length);
                    await gZipStream.FlushAsync();
                    gZipStream.Close();

                    gZippedBuffer = memoryStreamOutput.ToArray();
                }

                response.AddHeader("Content-Encoding", "gzip");
                response.ContentLength64 = gZippedBuffer.Length;

                Stream output = response.OutputStream;

                for (int i = 0; i < 10; i++)
                {
                    await output.WriteAsync(gZippedBuffer, i * 5000, 5000);
                    await Task.Delay(20);
                }

                await Task.Delay(2000);

                output.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task HandleNormalResponse(HttpListenerResponse response)
        {
            string responseString = JsonUtility.ToJson(new MockClassForSerialization());
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            output.Close();
        }

        private async Task HandleNormalGzippedResponse(HttpListenerResponse response)
        {
            try
            {
                string responseString = JsonUtility.ToJson(new BigMockClassForSerialization());

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                byte[] gZippedBuffer;

                {
                    using MemoryStream memoryStreamInput = new MemoryStream(buffer);
                    using MemoryStream memoryStreamOutput = new MemoryStream();
                    using GZipStream gZipStream = new GZipStream(memoryStreamOutput, CompressionLevel.Optimal, true);
                    await gZipStream.WriteAsync(buffer, 0, buffer.Length);
                    await gZipStream.FlushAsync();
                    gZipStream.Close();

                    gZippedBuffer = memoryStreamOutput.ToArray();
                }

                response.AddHeader("Content-Encoding", "gzip");
                response.ContentLength64 = gZippedBuffer.Length;
                Stream output = response.OutputStream;
                await output.WriteAsync(gZippedBuffer, 0, gZippedBuffer.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task HandlePostResponse(HttpListenerResponse response, HttpListenerRequest httpListenerRequest)
        {
            var dataLength = (int)httpListenerRequest.ContentLength64;
            var requestDataBuffer = new byte[dataLength];
            int readBytes;

            do
            {
                readBytes = await httpListenerRequest.InputStream.ReadAsync(requestDataBuffer, 0, dataLength);
            } while (readBytes != 0);

            var bytes = new byte[dataLength];
            requestDataBuffer.AsSpan(0, dataLength).CopyTo(bytes);

            string requestJson = Encoding.UTF8.GetString(bytes);
            var requestObject = JsonUtility.FromJson<MockClassForSerialization>(requestJson);

            string responseString = JsonUtility.ToJson(requestObject);
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            output.Close();
        }

        private async Task HandleRangesResponse(HttpListenerResponse response, HttpListenerRequest httpListenerRequest)
        {
            string responseString = JsonUtility.ToJson(new MockClassForSerialization());
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            var rangeHeaderString = httpListenerRequest.Headers.Get("Range");

            if (!string.IsNullOrWhiteSpace(rangeHeaderString))
            {
                RangeHeaderValue rangeHeader = RangeHeaderValue.Parse(rangeHeaderString);
                int rangeStart = (int)rangeHeader.Ranges.First().From!;
                int contentLength = buffer.Length - rangeStart;

                response.StatusCode = 206;
                response.AddHeader("Accept-Range", "bytes");
                response.AddHeader("Content-Range", $"{rangeStart}-{buffer.Length - 1}/{buffer.Length}");
                response.ContentLength64 = contentLength;

                Stream output = response.OutputStream;
                await output.WriteAsync(buffer, rangeStart, buffer.Length - rangeStart);
                output.Close();
            }
            else
            {
                try
                {
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;

                    var i = 15;
                    var offset = 0;

                    while (i-- > 0)
                    {
                        await output.WriteAsync(buffer, offset, 1);
                        offset++;

                        await Task.Delay(50);
                    }

                    output.Close();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task HandleEmptyRequest(HttpListenerResponse response)
        {
            response.ContentLength64 = 0;
            Stream output = response.OutputStream;
            await output.WriteAsync(Array.Empty<byte>(), 0, 0);
            output.Close();
        }
    }
}