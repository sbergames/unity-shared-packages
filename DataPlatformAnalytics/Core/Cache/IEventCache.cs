using System;
using System.Collections.Generic;

namespace SberGames.DataPlatform.Core
{
    public interface IEventCache : IDisposable
    {
        Action OnUnsentEventsLoaded { get; set; }
        
        void Add(string evetnId, string eventData);
        void Remove(string eventId);
        IEnumerable<KeyValuePair<string, string>> UnsentEvents();
    }
}