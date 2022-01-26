using System;
using System.Collections.Generic;
using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class DefaultEventBuilder : IEventBuilder
    {
        private const string EventNameKey = "event_name";
        private const string EventTimestampKey = "event_timestamp";
        private const string EventDateKey = "event_date";
        private const string EventTimeKey = "event_time";
        private const string AppVersionKey = "app_version";
        private const string ClientVersionKey = "client_version";
        private const string PlatformKey = "platform";
        private const string UserPseudoIdKey = "user_pseudo_id";
        private const string DeviceLanguageKey = "device_langauge";
        private const string DeviceHwModelKey = "device_hw_model";
        private const string DeviceOSKey = "device_OS";
        private const string AppBundleKey = "app_bundle_id";

        public void Build(ref EventData eventData, Dictionary<string, string> userParams)
        {
            if (eventData.data.ContainsKey(EventData.EventIdKey))
            {
                return;
            }
            
            foreach (var userParam in userParams)
            {
                eventData.AddData(userParam.Key, userParam.Value);
            }
            
            eventData.AddData(EventNameKey, eventData.eventName);
            
            AddDefaultParams(ref eventData);
        }
        
        private void AddDefaultParams(ref EventData data)
        {
            data.AddData(EventData.EventIdKey, GUIDGenerator.Generate());
            
            data.AddData(EventTimestampKey, ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString());
            data.AddData(EventDateKey, DateTime.UtcNow.ToString("yy-MM-dd"));
            data.AddData(EventTimeKey, DateTime.UtcNow.ToString("O"));
            
            data.AddData(ClientVersionKey, Application.version);
            data.AddData(AppVersionKey, Application.version);
            data.AddData(UserPseudoIdKey, GetPseudoUserId());
            data.AddData(AppBundleKey, Application.identifier);
            data.AddData(DeviceOSKey, SystemInfo.operatingSystem);
            data.AddData(DeviceHwModelKey, SystemInfo.deviceModel);
            data.AddData(DeviceLanguageKey, LanguageHelper.Get2LetterISOCodeFromSystemLanguage());
            
            #if UNITY_EDITOR
                data.AddData(PlatformKey, "editor");
            #elif UNITY_IOS
                data.AddData(PlatformKey, "ios");
            #elif UNITY_ANDROID
                data.AddData(PlatformKey, "android");
            #endif
        }
        
        private string GetPseudoUserId()
        {
	        var userPseudoId = PlayerPrefs.GetString(UserPseudoIdKey, "");
            if (userPseudoId.Equals(""))
            {
                userPseudoId = GUIDGenerator.Generate();
                PlayerPrefs.SetString(UserPseudoIdKey, userPseudoId);
            }

            return userPseudoId;
        }
    }
}
