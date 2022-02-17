using SberGames.DataPlatform;
using UnityEngine;
using SberGames.DataPlatform.Core.Net;
using System;

namespace SberGames.DataPlatform.Core
{
    public interface IDataPlatformAnalyticsImpl : IDisposable
    {
        void SetUserProperty(string key, string value);

        void RemoveUserProperty(string key);

        void SendEvent(EventData eventData);

        void StartSession();

        void IsGetDeviceId(bool isGet);
    }
}