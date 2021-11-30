namespace SberGames.Utils.DataDownloader.Configuration
{
    using System.Net.Http;

    public readonly struct DataDownloadRequestInfo
    {
        public readonly HttpMethod Method;
        public readonly HttpContent? Content;

        public DataDownloadRequestInfo(HttpMethod method, HttpContent? content)
        {
            Method = method;
            Content = content;
        }
    }
}