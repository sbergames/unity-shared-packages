using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SberGames.DataPlatform.Core.Net;
using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class DataPlatformAnalyticsDotNetImpl : IDataPlatformAnalyticsImpl
    {
        private const int MaxEventAtOnce = 10;
        private const int TimeoutBetweenResend = 10_000;
        
        private const string SessionIdKey = "session_id";
        private const string FirstLaunchKey = "first_launch";

        private Dictionary<string, string> userParams;
        private List<string> locked = new List<string>();
        
        private JsonEventDataSerializer jsonEventDataSerializer;

        private IEventSender eventSender = null;
        private IEventCache eventCache = null;
        private IEventBuilder eventBuilder = null;

        private int sendingErrorCountFromLastSuccess = 0;

        private int CurrentTimeoutBetweenResend => TimeoutBetweenResend * 
                                                   (sendingErrorCountFromLastSuccess < 3 ? sendingErrorCountFromLastSuccess + sendingErrorCountFromLastSuccess + 1 : 6);

        public DataPlatformAnalyticsDotNetImpl(IEventSender _eventSender)
        {
            jsonEventDataSerializer = new JsonEventDataSerializer();
            userParams = new Dictionary<string, string>();
            eventSender = _eventSender;
            eventCache = new FileEventCache();
            eventBuilder = new DefaultEventBuilder();

            StartResendProcess();
        }

        public void StartSession()
        {
            sendingErrorCountFromLastSuccess = 0;
            
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
            eventCache.Add(eventData.EventId, serializedEventData);
            Sending(eventData.EventId, serializedEventData);
        }

        public void Dispose()
        {
            eventCache?.Dispose();
            eventCache = null;
        }
        
        private async Task<bool> Sending(string eventId, string eventData)
        {
            locked.Add(eventId);
            
            SendingResult result = await eventSender.Send(eventData);
            
            locked.Remove(eventId);
            
            if (result.IsSuccess)
            {
                sendingErrorCountFromLastSuccess = 0;
                eventCache.Remove(eventId);

                // ???????? ?? ?????????????? ???????
                int unsentEventsCount = eventCache.UnsentEvents().Count();
                if (unsentEventsCount > 1)
                {
                    StartResendProcess();
                }
            }
            else
            {
                sendingErrorCountFromLastSuccess++;
                Debug.LogWarning($"DataPlatform Analytics event sending error: {result.Error}");
                return false;
            }

            return true;
        }
        
        private async Task<bool> Sending(List<string> eventIds, List<string> eventDatas)
        {
            locked.AddRange(eventIds);
            
            SendingResult result = await eventSender.Send(eventDatas);

            foreach (var eventId in eventIds)
            {
                locked.Remove(eventId);
            }
            
            if (result.IsSuccess)
            {
                sendingErrorCountFromLastSuccess = 0;
                foreach (var eventId in eventIds)
                {
                    eventCache.Remove(eventId);
                }
            }
            else
            {
                sendingErrorCountFromLastSuccess++;
                Debug.LogWarning($"DataPlatform Analytics event sending error: {result.Error}");
                return false;
            }

            return true;
        }

        private async void StartResendProcess()
        {
            while (eventCache != null)
            {
                await Task.Delay(CurrentTimeoutBetweenResend);

                bool isNeedNextTry = false;
                do
                {
                    isNeedNextTry = await TrySendCashedBatch();
                } while (isNeedNextTry);
            }
        }
        
        private async Task<bool> TrySendCashedBatch()
        {
            if (eventCache != null && eventCache.UnsentEvents().Count() > 0)
            {
                List<string> eventIds = new List<string>();
                List<string> eventDatas = new List<string>();

                var enumerator = eventCache.UnsentEvents().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (!locked.Contains(enumerator.Current.Key))
                    {
                        eventIds.Add(enumerator.Current.Key);
                        eventDatas.Add(enumerator.Current.Value);

                        if (eventIds.Count() >= MaxEventAtOnce)
                        {
                            break;
                        }
                    }
                }
                enumerator.Dispose();

                if (eventIds != null && eventIds.Count > 0)
                {
                    return await Sending(eventIds, eventDatas);
                }
            }
            
            return false;
        }
    }
}