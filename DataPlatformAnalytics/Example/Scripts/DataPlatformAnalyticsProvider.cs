using UnityEngine;

namespace SberGames.DataPlatform.Example
{
    public class DataPlatformAnalyticsProvider
    {
        private static DataPlatformAnalyticsProvider _instance = null;
        
        private DataPlatformAnalytics dataPlatformAnalytics = null;

        public static DataPlatformAnalyticsProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataPlatformAnalyticsProvider();
                    _instance.Initialize();
                }

                return _instance;
            }
        }
        
        private void Initialize()
        {
            var dataPlatformSettings = Resources.Load<DataPlatformSettings>("DataPlatformSettingsExample");
            
            dataPlatformAnalytics = new DataPlatformAnalytics();
            dataPlatformAnalytics.Initialize(dataPlatformSettings.ApiKey, dataPlatformSettings.BaseUri);
            
            dataPlatformAnalytics.SetUserProperty("user_id", "example_user");
        }
        
        public void SendButton1ClickedEvent()
        {
            EventData eventData = new EventData("button_1_clicked")
                .AddData("some_param", "some_value");
            dataPlatformAnalytics.SendEvent(eventData);
        }
        
        public void SendButton2ClickedEvent()
        {
            EventData eventData = new EventData("button_2_clicked")
                .AddData("some_param", "some_value");
            dataPlatformAnalytics.SendEvent(eventData);
        }
    }
}
