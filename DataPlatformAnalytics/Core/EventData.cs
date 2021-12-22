using System.Collections.Generic;
using UnityEngine;

namespace SberGames.DataPlatform
{
    public struct EventData
    {
        public static string EventIdKey = "dp_event_id";
        
        public readonly Dictionary<string, string> data;
        public readonly string eventName;

        public string EventId => data[EventIdKey] ?? "";
        
        
        public EventData(string _eventName)
        {
            eventName = _eventName;
            data = new Dictionary<string, string>();
        }
        
        public EventData AddData(string key, string value)
        {
            if (data.ContainsKey(key))
            {
                Debug.Log($"Duplicate user property: {key} in event: {eventName}");
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }
            
            return this;
        }
    }
}
