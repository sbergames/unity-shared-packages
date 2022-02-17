using System;
using System.Collections.Generic;
using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class DefaultEventBuilder : IEventBuilder
    {
        private const string SessionIdKey = "session_id";
        private const string EventNameKey = "event_name";
        private const string EventTimestampKey = "event_timestamp";
        private const string ClientVersionKey = "client_version";
        private const string PlatformKey = "platform";
        private const string UserPseudoIdKey = "install_id";
        private const string DeviceLanguageKey = "device_langauge";
        private const string DeviceHwModelKey = "device_hw_model";
        private const string DeviceOSKey = "device_OS";
        private const string AppBundleKey = "app_bundle_id";
        private const string DeviceUniqueId = "device_id";
        private const string LocalDatetime = "local_datetime";

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

            Validate(eventData);
        }

        private void Validate(EventData data)
        {
            if (!data.IsData(EventNameKey))
            {
                Debug.Log($"Отсуствует ключ имени события {EventNameKey}");
            }

            if (!data.IsData(SessionIdKey))
            {
                Debug.Log($"Отсутсвует ключ {SessionIdKey}. Для создания {SessionIdKey} необходимо вызвать метод StartSession()");
            }

            string[] key_list = {
                EventTimestampKey, ClientVersionKey, PlatformKey, UserPseudoIdKey, DeviceLanguageKey,
                DeviceHwModelKey, DeviceHwModelKey, DeviceOSKey, AppBundleKey, DeviceUniqueId
            };

            if (!data.IsDatas(key_list))
            {
                Debug.Log("Отсутствуют обязательные ключи, необходимые в отправляемом сообщении о событии");
            }
        }

        private void AddDefaultParams(ref EventData data)
        {
            data.AddData(EventData.EventIdKey, GUIDGenerator.Generate());
            
            data.AddData(EventTimestampKey, ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds().ToString());
            data.AddData(LocalDatetime, DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            data.AddData(ClientVersionKey, Application.version);
            data.AddData(UserPseudoIdKey, GetPseudoUserId());
            data.AddData(AppBundleKey, Application.identifier);
            data.AddData(DeviceOSKey, SystemInfo.operatingSystem);
            data.AddData(DeviceHwModelKey, SystemInfo.deviceModel);
            data.AddData(DeviceLanguageKey, LanguageHelper.Get2LetterISOCodeFromSystemLanguage());
            data.AddData(DeviceUniqueId, SystemInfo.deviceUniqueIdentifier.Replace("-", ""));
            
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
