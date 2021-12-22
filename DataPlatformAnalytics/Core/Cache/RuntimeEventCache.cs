using System;
using System.Collections.Generic;

namespace SberGames.DataPlatform.Core
{
    public class RuntimeEventCache : IEventCache
    {
        private Dictionary<string, string> cache = new Dictionary<string, string>();
        
        public Action OnUnsentEventsLoaded { get; set; }
        
        public void Add(string evetnId, string eventData)
        {
            cache.Add(evetnId, eventData);
        }

        public void Remove(string eventId)
        {
            cache.Remove(eventId);
        }

        public IEnumerable<KeyValuePair<string, string>> UnsentEvents()
        {
            return cache;
        }
        
        public void Dispose()
        {
        }
    }
}