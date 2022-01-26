using SberGames.DataPlatform.Core;
using SberGames.DataPlatform.Core.Net;
using UnityEngine;

namespace SberGames.DataPlatform
{
    public class DataPlatformAnalytics : IDataPlatformAnalytics
    {
        private DataPlatformAnalyticsImpl dataPlatformAnalyticsImpl = null;

        public void Initialize(string _apiKey, string _host)
        {
            var eventSender = new HttpEventSender();
            eventSender.Initialization(_apiKey, _host);

            dataPlatformAnalyticsImpl = new DataPlatformAnalyticsImpl();
            dataPlatformAnalyticsImpl.Initialize(eventSender);

            var go = new GameObject();
            var dataPlatformUnityObject = go.AddComponent<DataPlatformUnityObject>();
            dataPlatformUnityObject.Initialize(dataPlatformAnalyticsImpl);
        }
        
        public void SetUserProperty(string key, string value)
        {
            dataPlatformAnalyticsImpl.SetUserProperty(key, value);
        }

        public void SendEvent(EventData data)
        {
            dataPlatformAnalyticsImpl.SendEvent(data);
        }
    }
}
