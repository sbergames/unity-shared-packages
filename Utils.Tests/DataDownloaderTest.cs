namespace Utils.Tests
{
    using System;
    using System.Buffers;
    using System.Collections;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SampleData.DataDownloader;
    using SberGames.Utils.DataDownloader;
    using SberGames.Utils.DataDownloader.Configuration;
    using SberGames.Utils.DataDownloader.DefaultDownloaders.FixedBuffer;
    using SberGames.Utils.DataDownloader.DefaultDownloaders.UnlimitedBuffer;
    using SberGames.Utils.DataDownloader.DefaultProgressConsumers;
    using SberGames.Utils.DataDownloader.Result;
    using UnityEngine;
    using UnityEngine.TestTools;

    [TestFixture]
    public class DataDownloaderTest
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
            };
            httpClient = new HttpClient(handler, true);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            httpClient.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            dataDownloader = new DataDownloader(httpClient, 5_000);
            
            httpServer = new SimpleServer();
            httpServer.StartListen();
        }
        
        [TearDown]
        public void TearDown()
        {
            httpServer.Destroy();
        }

        private HttpClient httpClient = default!;
        private SimpleServer httpServer = default!;
        private DataDownloader dataDownloader = default!;

        [UnityTest]
        public IEnumerator BadAddressFailTest()
        {
            string url = @"https://some.inexistant.site";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            // This test doesn't work under VPN.
            Assert.True(result.IsNetworkError);
            Assert.False(result.IsSuccess);
            Assert.False(result.IsCancelled);
            Assert.False(result.IsTimeout);
            Assert.IsNotNull(result.Error);
            Assert.False(result.StatusCode.HasValue);
        }
        
        [UnityTest]
        public IEnumerator BadPortFailWithServerNotAnsweringTest()
        {
            string url = @"http://localhost:9000";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        10_000,
                        10_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.True(result.IsNetworkError);
            Assert.False(result.IsSuccess);
            Assert.False(result.IsCancelled);
            Assert.False(result.IsTimeout);
            Assert.IsNotNull(result.Error);
            Assert.False(result.StatusCode.HasValue);
        }
        
        [UnityTest]
        public IEnumerator BadPortFailWithHeadersTimeoutTest()
        {
            string url = @"http://localhost:9000";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        1_000,
                        1_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.False(result.IsSuccess);
            Assert.False(result.IsCancelled);
            Assert.True(result.IsTimeout);
            Assert.IsNotNull(result.Error);
            Assert.False(result.StatusCode.HasValue);
        }
        
        [UnityTest]
        public IEnumerator GoodServerHeadersTimeoutFailTest()
        {
            string requestType = "headersTimeout";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        1_000,
                        1_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsTimeout);
            Assert.False(result.IsCancelled);
            Assert.IsNotNull(result.Error);
            Assert.False(result.StatusCode.HasValue);
        }
        
        [UnityTest]
        public IEnumerator GoodServer404FailTest()
        {
            string requestType = "404";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        1_000,
                        1_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.True(result.IsServerError);
            Assert.False(result.IsSuccess);
            Assert.False(result.IsTimeout);
            Assert.False(result.IsCancelled);
            Assert.IsNotNull(result.Error);
            Assert.AreEqual(404, result.StatusCode);
        }
        
        [UnityTest]
        public IEnumerator GoodServerGoodHeadersAndNoDataFailTest()
        {
            string requestType = "goodHeadersNoData";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        1_000,
                        1_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsTimeout);
            Assert.False(result.IsCancelled);
            Assert.IsNotNull(result.Error);
            Assert.AreEqual(200, result.StatusCode);
        }

        [UnityTest]
        public IEnumerator DownloadSomeNormalDataTest()
        {
            string requestType = "normal";
            string url = $@"http://localhost:9999?requestType={requestType}";

            byte[] buffer = new byte[5000];
            var downloader = new FixedBufferDataDownloader(httpClient);
            Task<BufferDataDownloadResult> task = downloader.DownloadAsync(
                new FixedBufferDataDownloadParameters(
                    url,
                    CancellationToken.None),
                buffer);

            yield return WaitForTask(task);

            var result = task.Result;

            MockClassForSerialization? responseObject = null;

            if (result.Buffer.HasValue)
            {
                Memory<byte> resultData = result.Buffer.Value;
                var bytes = new byte[resultData.Length];
                resultData.Span.CopyTo(bytes);

                string responseJson = Encoding.UTF8.GetString(bytes);
                responseObject = JsonUtility.FromJson<MockClassForSerialization>(responseJson);
            }

            Assert.False(result.Result.IsNone);
            Assert.True(result.Result.IsSuccess);
            Assert.IsNull(result.Result.Error);
            Assert.AreEqual(200, result.Result.StatusCode);
            Assert.IsNotNull(responseObject);

            if (responseObject != null)
            {
                Assert.AreEqual(5, responseObject.A);
                Assert.AreEqual(6, responseObject.B);
                Assert.AreEqual(7, responseObject.C);
                Assert.AreEqual(8, responseObject.D);
            }
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeNormalDataWithRangesTest()
        {
            string requestType = "ranges";
            string url = $@"http://localhost:9999?requestType={requestType}";

            var tokenSource = new CancellationTokenSource();
            
            var downloader = new FixedBufferDataDownloader(httpClient);
            Task<BufferDataDownloadResult> initialTask = downloader.DownloadAsync(
                new FixedBufferDataDownloadParameters(
                    url,
                    tokenSource.Token),
                5000);

            var delayTask = Task.Delay(500);
            yield return WaitForTask(delayTask);

            tokenSource.Cancel();
            
            yield return WaitForTask(initialTask);
            
            var firstPartResult = initialTask.Result;
            
            Assert.False(firstPartResult.Result.IsNone);
            Assert.False(firstPartResult.Result.IsSuccess);
            Assert.True(firstPartResult.Result.IsCancelled);
            Assert.IsNotNull(firstPartResult.Result.Error);
            Assert.AreEqual(200, firstPartResult.Result.StatusCode);

            var bytesDownloadedFirstPart = firstPartResult.Result.DownloadedBytesCount;
            
            Assert.Greater(bytesDownloadedFirstPart, 0);

            var firstPartBuffer = new byte[bytesDownloadedFirstPart];

            if (firstPartResult.Buffer != null)
            {
                var downloadedBytes = firstPartResult.Buffer.Value.Span;
                downloadedBytes.CopyTo(firstPartBuffer);
            }

            downloader = new FixedBufferDataDownloader(httpClient);
            Task<BufferDataDownloadResult> task = downloader.DownloadAsync(
                new FixedBufferDataDownloadParameters(
                    url,
                    CancellationToken.None,
                    new DataDownloadAdditionalParameters(
                        rangesInfo: new DataDownloadRangesInfo(bytesDownloadedFirstPart))),
                5000);
            
            yield return WaitForTask(task);

            var secondPartResult = task.Result;
            var bytesDownloadedSecondPart = secondPartResult.Result.DownloadedBytesCount;
            
            Assert.Greater(bytesDownloadedSecondPart, 0);
            
            var wholeBuffer = new byte[bytesDownloadedFirstPart + bytesDownloadedSecondPart];
            firstPartBuffer.AsSpan().CopyTo(wholeBuffer.AsSpan(0, (int)bytesDownloadedFirstPart));

            if (secondPartResult.Buffer != null)
            {
                var downloadedBytes = secondPartResult.Buffer.Value.Span;
                downloadedBytes.CopyTo(wholeBuffer.AsSpan((int)bytesDownloadedFirstPart, (int)bytesDownloadedSecondPart));
            }

            string responseJson = Encoding.UTF8.GetString(wholeBuffer);
            MockClassForSerialization? responseObject = JsonUtility.FromJson<MockClassForSerialization>(responseJson);

            Assert.False(secondPartResult.Result.IsNone);
            Assert.True(secondPartResult.Result.IsSuccess);
            Assert.IsNull(secondPartResult.Result.Error);
            Assert.AreEqual(206, secondPartResult.Result.StatusCode);
            Assert.IsNotNull(responseObject);

            if (responseObject != null)
            {
                Assert.AreEqual(5, responseObject.A);
                Assert.AreEqual(6, responseObject.B);
                Assert.AreEqual(7, responseObject.C);
                Assert.AreEqual(8, responseObject.D);
            }
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeNormalDataWithPostTest()
        {
            string requestType = "post";
            string url = $@"http://localhost:9999?requestType={requestType}";

            string requestString = JsonUtility.ToJson(new MockClassForSerialization(2, 12, 3, 56));
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestString);
            ByteArrayContent byteArrayHttpContent = new ByteArrayContent(requestBytes);
            
            byte[] receiveBuffer = new byte[5000];
            var downloader = new FixedBufferDataDownloader(httpClient);
            Task<BufferDataDownloadResult> task = downloader.DownloadAsync(
                new FixedBufferDataDownloadParameters(
                    url,
                    CancellationToken.None,
                new DataDownloadAdditionalParameters(
                    requestInfo: new DataDownloadRequestInfo(HttpMethod.Post, byteArrayHttpContent))),
                receiveBuffer);

            yield return WaitForTask(task);

            var result = task.Result;

            MockClassForSerialization? responseObject = null;

            if (result.Buffer.HasValue)
            {
                Memory<byte> resultData = result.Buffer.Value;
                var bytes = new byte[resultData.Length];
                resultData.Span.CopyTo(bytes);

                string responseJson = Encoding.UTF8.GetString(bytes);
                responseObject = JsonUtility.FromJson<MockClassForSerialization>(responseJson);
            }

            Assert.False(result.Result.IsNone);
            Assert.True(result.Result.IsSuccess);
            Assert.IsNull(result.Result.Error);
            Assert.AreEqual(200, result.Result.StatusCode);
            Assert.IsNotNull(responseObject);

            if (responseObject != null)
            {
                Assert.AreEqual(2, responseObject.A);
                Assert.AreEqual(12, responseObject.B);
                Assert.AreEqual(3, responseObject.C);
                Assert.AreEqual(56, responseObject.D);
            }
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeNormalGZippedDataTest()
        {
            string requestType = "normalGzipped";
            string url = $@"http://localhost:9999?requestType={requestType}";

            byte[] buffer = new byte[5000];
            var downloader = new FixedBufferDataDownloader(httpClient);
            Task<BufferDataDownloadResult> task = downloader.DownloadAsync(
                new FixedBufferDataDownloadParameters(
                    url,
                    CancellationToken.None),
                buffer);

            yield return WaitForTask(task);

            var result = task.Result;

            BigMockClassForSerialization? responseObject = null;

            if (result.Buffer.HasValue)
            {
                Memory<byte> resultData = result.Buffer.Value;
                var bytes = new byte[resultData.Length];
                resultData.Span.CopyTo(bytes);

                string responseJson = Encoding.UTF8.GetString(bytes);
                responseObject = JsonUtility.FromJson<BigMockClassForSerialization>(responseJson);
            }

            Assert.False(result.Result.IsNone);
            Assert.True(result.Result.IsSuccess);
            Assert.IsNull(result.Result.Error);
            Assert.AreEqual(200, result.Result.StatusCode);
            Assert.IsNotNull(responseObject);

            if (responseObject != null)
            {
                Assert.AreEqual(5, responseObject.A);
                Assert.AreEqual(6, responseObject.B);
                Assert.AreEqual(7, responseObject.C);
                Assert.AreEqual(8, responseObject.D);
            }
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataAtOnceTest()
        {
            string requestType = "bigObject";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        2_000,
                        2_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.True(result.IsSuccess);
            Assert.True(result.Error == null);
            Assert.True(result.StatusCode == 200);
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataByChunksWithProgressTest()
        {
            var currentThreadManagedId = Thread.CurrentThread.ManagedThreadId;

            float totalProgress = default;
            var simpleProgressConsumer = new SimpleProgressConsumer(
                progress =>
                {
                    Assert.AreNotEqual(currentThreadManagedId, Thread.CurrentThread.ManagedThreadId);
                    totalProgress = progress;
                });

            string requestType = "bigObjectByChunks";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    simpleProgressConsumer,
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        2_000,
                        2_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.True(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1.0, totalProgress, 0.001);
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataByChunksWithUnlimitedProgressConsumerTest()
        {
            string requestType = "bigObjectByChunks";
            string url = $@"http://localhost:9999?requestType={requestType}";

            var downloader = new UnlimitedBufferDataDownloader(httpClient);
            Task<UnlimitedBufferDataDownloadResult> task = downloader.DownloadAsync(
                new UnlimitedBufferDataDownloadParameters(
                    url,
                    CancellationToken.None,
                    new DataDownloadAdditionalParameters(
                        new DataDownloadTimeouts(
                            30_000,
                            2_000,
                            2_000))));

            yield return WaitForTask(task);

            UnlimitedBufferDataDownloadResult result = task.Result;

            Assert.False(result.Result.IsNone);
            Assert.True(result.Result.IsSuccess);
            Assert.IsNull(result.Result.Error);
            Assert.AreEqual(200, result.Result.StatusCode);

            (IMemoryOwner<byte> memoryOwner, int bytesInMemory)? resultData = result.Data;

            if (resultData != null)
            {
                (IMemoryOwner<byte> memoryOwner, int bytesInMemory) = resultData.Value;
                
                Assert.AreEqual(60 * 1024 * 1024, bytesInMemory);
                Assert.AreEqual(1, memoryOwner.Memory.Span[0]);
                Assert.AreEqual(1, memoryOwner.Memory.Span[30 * 1024 * 1024]);
                Assert.AreEqual(1, memoryOwner.Memory.Span[60 * 1024 * 1024 - 1]);
                
                memoryOwner.Dispose();
            }
            
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataByChunksWithProgressOnSpecificThreadTest()
        {
            var currentThreadManagedId = Thread.CurrentThread.ManagedThreadId;
            
            float totalProgress = default;
            using var contextBindedProgressConsumer = new ContextBindedProgressConsumer(
                progress =>
                {
                    Assert.AreEqual(currentThreadManagedId, Thread.CurrentThread.ManagedThreadId);
                    totalProgress = progress;
                }, SynchronizationContext.Current);

            string requestType = "bigObjectByChunks";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    contextBindedProgressConsumer,
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        2_000,
                        2_000)));

            yield return WaitForTask(task);
            
            // Waiting for the progress tick
            yield return WaitForTask(Task.Delay(50));

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.True(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1.0, totalProgress, 0.001);
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataByChunksWithUnexpectedStopFailTest()
        {
            string requestType = "bigObjectByChunksWithUnexpectedStop";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        1_000,
                        1_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsTimeout);
            Assert.IsNotNull(result.Error);
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataGZippedByChunksWithUnexpectedStopFailTest()
        {
            string requestType = "bigObjectByChunksGZippedWithUnexpectedStop";
            string url = $@"http://localhost:9999?requestType={requestType}";

            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    CancellationToken.None),
                new DataDownloadAdditionalParameters(
                    new DataDownloadTimeouts(
                        30_000,
                        1_000,
                        1_000)));

            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsTimeout);
            Assert.IsNotNull(result.Error);
        }
        
        [UnityTest]
        public IEnumerator DownloadSomeBigDataWithCancelTest()
        {
            string requestType = "bigObjectByChunksWithUnexpectedStop";
            string url = $@"http://localhost:9999?requestType={requestType}";

            var cancellationSource = new CancellationTokenSource();
            
            Task<DataDownloadResult> task = DownloadDataWithParams(
                new DataDownloadParameters(
                    url,
                    new EmptyProgressConsumer(),
                    cancellationSource.Token));

            yield return WaitForTask(Task.Delay(500));
            
            cancellationSource.Cancel();
            
            yield return WaitForTask(task);

            DataDownloadResult result = task.Result;

            Assert.False(result.IsNone);
            Assert.False(result.IsSuccess);
            Assert.False(result.IsTimeout);
            Assert.True(result.IsCancelled);
            Assert.IsNotNull(result.Error);
        }

        private Task<DataDownloadResult> DownloadDataWithParams(DataDownloadParameters dataDownloadParameters, DataDownloadAdditionalParameters? additionalParameters = null)
        {
            Task<DataDownloadResult> task;
            
            if (additionalParameters.HasValue)
            {
                task = Task.Run(async () => await dataDownloader.Download(dataDownloadParameters, additionalParameters.Value));
            }
            else
            {
                task = Task.Run(async () => await dataDownloader.Download(dataDownloadParameters));
            }

            return task;
        }

        private static IEnumerator WaitForTask(Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception;
            }
        }
    }
}