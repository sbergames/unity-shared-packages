using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SberGames.DataPlatform.Core.Net;
using System;
using SberGames.DataPlatform;
using System.Runtime.InteropServices;
using AOT;

namespace SberGames.DataPlatform.Core
{
    public class DataPlatformAnalyticsIOS : IDataPlatformAnalyticsImpl
    {
        public delegate void DataPlatformCallbackDelegate(string text);
        [DllImport("__Internal")]
        public static extern void setupSettingsIOS(string APIkey, string host);
        [DllImport("__Internal")]
        public static extern void setCallbackDelegate(DataPlatformCallbackDelegate cb);
        [DllImport("__Internal")]
        public static extern void sendEventIOS(string data, string eventName, string eventId);

        private IEventBuilder eventBuilder;
        private Dictionary<string, string> userParams;
        private JsonEventDataSerializer jsonEventDataSerializer;
        private const string SessionIdKey = "session_id";
        private const string FirstLaunchKey = "first_launch";


        public DataPlatformAnalyticsIOS(string APIkey, string host) {
            DataPlatformAnalyticsIOS.setupSettingsIOS(APIkey, host);
            DataPlatformAnalyticsIOS.setCallbackDelegate(DataPlatformAnalyticsCallback.Callback);
            eventBuilder = new DefaultEventBuilder();
            jsonEventDataSerializer = new JsonEventDataSerializer();
            userParams = new Dictionary<string, string>();
            SetUserProperty("user_id", "example_user");
        }

        public void StartSession()
        {
            SetUserProperty(SessionIdKey, GUIDGenerator.Generate());

            if (PlayerPrefs.GetInt(FirstLaunchKey, 0) == 0)
            {
                SetUserProperty(FirstLaunchKey, "true");
                PlayerPrefs.SetInt(FirstLaunchKey, 1);
            }
            else
            {
                RemoveUserProperty(FirstLaunchKey);
            }
        }

        public void SetUserProperty(string key, string value)
        {
            if (userParams.ContainsKey(key))
            {
                userParams[key] = value;
            }
            else
            {
                userParams.Add(key, value);
            }
        }

        public void RemoveUserProperty(string key)
        {
            userParams.Remove(key);
        }


        public void SendEvent(EventData eventData)
        {
            eventBuilder.Build(ref eventData, userParams);
            var serializedEventData = jsonEventDataSerializer.Serialize(eventData);
            Debug.Log("SendEvent IOS (EventData eventData)");
            sendEventIOS(eventId: eventData.EventId, data: serializedEventData, eventName: eventData.eventName);
        }


        public void Dispose()
        {
            
        }
    }

    public class DataPlatformAnalyticsCallback
    {

        [MonoPInvokeCallback(typeof(DataPlatformAnalyticsIOS.DataPlatformCallbackDelegate))]

        public static void Callback(string text)
        {
            Debug.Log("This static function DataPlatformAnalyticsCallback Callback has been called from iOS");
            Debug.Log(text);

        }

    }
}


    
