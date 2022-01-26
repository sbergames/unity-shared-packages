using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SberGames.DataPlatform.Core.Net;
using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class DataPlatformAnalyticsImpl : IDisposable
    {
        private const int MaxEventAtOnce = 10;

        private const string SessionIdKey = "session_id";
        private const string FirstLaunchKey = "first_launch";

        private Dictionary<string, string> userParams;
        private List<string> locked = new List<string>();
        
        private JsonEventDataSerializer jsonEventDataSerializer;

        private IEventSender eventSender;
        private IEventCache eventCache;
        private IEventBuilder eventBuilder;
        
        public void Initialize(IEventSender _eventSender)
        {
            jsonEventDataSerializer = new JsonEventDataSerializer();
            userParams = new Dictionary<string, string>();
            eventSender = _eventSender;
            eventCache = new FileEventCache();
            eventCache.OnUnsentEventsLoaded += OnUnsentEventsLoaded;
            eventBuilder = new DefaultEventBuilder();
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
            eventCache.Add(eventData.EventId, serializedEventData);
            Sending(eventData.EventId, serializedEventData);
        }

        public void Dispose()
        {
            eventCache?.Dispose();
        }
        
        private async void Sending(string eventId, string eventData)
        {
            locked.Add(eventId);
            
            SendingResult result = await eventSender.Send(eventData);
            
            locked.Remove(eventId);
            
            if (result.IsSuccess)
            {
                eventCache.Remove(eventId);
            }
            else
            {
                Debug.LogWarning($"DataPlatform Analytics event sending error: {result.Error}");
                RetrySending();
            }
        }
        
        private async Task Sending(List<string> eventIds, List<string> eventDatas)
        {
            locked.AddRange(eventIds);
            
            SendingResult result = await eventSender.Send(eventDatas);

            foreach (var eventId in eventIds)
            {
                locked.Remove(eventId);
            }
            
            if (result.IsSuccess)
            {
                foreach (var eventId in eventIds)
                {
                    eventCache.Remove(eventId);
                }
            }
            else
            {
                Debug.LogWarning($"DataPlatform Analytics event sending error: {result.Error}");
                RetrySending();
            }
        }

        private async void RetrySending()
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
                    
                    if (eventIds.Count >= MaxEventAtOnce)
                    {
                        await Sending(eventIds, eventDatas);
                        eventIds.Clear();
                        eventDatas.Clear();
                    }
                }
            }
            enumerator.Dispose();

            if (eventIds.Count > 0)
            {
                await Sending(eventIds, eventDatas);
            }
        }

        private void OnUnsentEventsLoaded()
        {
            RetrySending();
        }
    }
}