using SberGames.DataPlatform.Core;
using SberGames.DataPlatform.Core.Net;
using UnityEngine;

namespace SberGames.DataPlatform
{
    public class DataPlatformAnalytics : IDataPlatformAnalytics
    {
        private IDataPlatformAnalyticsImpl dataPlatformAnalyticsImpl = null;

        public void Initialize(string _apiKey, string _host)
        {

//#if UNITY_EDITOR && UNITY_ANDROID
            var eventSender = new HttpEventSender();
            eventSender.Initialization(_apiKey, _host);
            dataPlatformAnalyticsImpl = new DataPlatformAnalyticsDotNetImpl(eventSender);
//#elif UNITY_IOS
            //dataPlatformAnalyticsImpl = new DataPlatformAnalyticsIOS(_apiKey, _host);
//#endif

            var go = new GameObject();
            var dataPlatformUnityObject = go.AddComponent<DataPlatformUnityObject>();
            dataPlatformUnityObject.Initialize(dataPlatformAnalyticsImpl);
        }

        public void IsGetDeviceId(bool isGet)
        {
            dataPlatformAnalyticsImpl.IsGetDeviceId(isGet);
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
