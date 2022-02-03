using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SberGames.DataPlatform.Example
{
    public class DataPlatformAnalyticsWrapper
    {
        private string apiKey = "UdVvIOmJuv-oY9OXwfXhHVLq1Hv8YwWoOkIbwAPQMSDgF1KjChPx1n63981zoWL6";
        private string baseUri = "https://api.sbergames.network/api/events/batch?api_key={API_KEY}";

        private static DataPlatformAnalyticsWrapper _instance = null;

        private DataPlatformAnalytics dataPlatformAnalytics = null;

        public static DataPlatformAnalyticsWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataPlatformAnalyticsWrapper();
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        private void Initialize()
        {
            dataPlatformAnalytics = new DataPlatformAnalytics();
            dataPlatformAnalytics.Initialize(apiKey, baseUri);
            dataPlatformAnalytics.SetUserProperty("user_id", "example_user");
        }

        
        public void SendButtonClickedEvent()
        {
            EventData eventData = new EventData("button_clicked")
                .AddData("some_param", "some_value");

            dataPlatformAnalytics.SendEvent(eventData);
        }
    }
}