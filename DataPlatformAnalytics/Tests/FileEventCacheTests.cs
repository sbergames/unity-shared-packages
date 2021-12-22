using System.Collections;
using System.Linq;
using NUnit.Framework;

namespace SberGames.DataPlatform.Core.Tests
{
    public class FileEventCacheTests
    {
        private const string event1Id = "sdfh98";
        private const string event1Data = "{\"someData\":1}";
            
        private const string event2Id = "sdmj9y";
        private const string event2Data = "{\"someData\":2}";

        bool dataLoaded = false;
        
        [Test]
        public void CashingTest()
        {
            IEventCache cache = new FileEventCache();
            
            cache.Add(event1Id, event1Data);
            cache.Add(event2Id, event2Data);
            
            cache.Remove(event1Id);

            var cachedEvents = cache.UnsentEvents().ToList();
            
            Assert.IsTrue(cachedEvents.Count == 1);
            Assert.IsTrue(cachedEvents[0].Key.Equals(event2Id));
            Assert.IsTrue(cachedEvents[0].Value.Equals(event2Data));
            
            cache.Remove(event2Id);
            
            cache.Dispose();
        }
        
        [Test(ExpectedResult = null)]
        public IEnumerator CashingBeetwenSessionsTest()
        {
            yield return GenerateFileCache();

            yield return CheckEventsStore();

            yield return CleanCache();
        }

        private IEnumerable CleanCache()
        {
            var cacheSession = new FileEventCache();
            cacheSession.OnUnsentEventsLoaded += OnUnsentEventsLoaded;
            
            dataLoaded = false;
            while (!dataLoaded)
                yield return null;

            cacheSession.OnUnsentEventsLoaded -= OnUnsentEventsLoaded;
            
            cacheSession.Remove(event2Id);
            cacheSession.Dispose();
                
            while (!cacheSession.IsFilesClosed)
                yield return null;
        }
        
        private IEnumerable CheckEventsStore()
        {
            var cacheSession = new FileEventCache();
            cacheSession.OnUnsentEventsLoaded += OnUnsentEventsLoaded;
            
            dataLoaded = false;
            while (!dataLoaded)
                yield return null;

            cacheSession.OnUnsentEventsLoaded -= OnUnsentEventsLoaded;
            
            var cachedEvents = cacheSession.UnsentEvents().ToList();
            cacheSession.Dispose();
                
            while (!cacheSession.IsFilesClosed)
                yield return null;
            
            Assert.AreEqual(cachedEvents.Count, 1);
            Assert.AreEqual(cachedEvents[0].Key, event2Id);
            Assert.AreEqual(cachedEvents[0].Value,event2Data);
        }
        
        private IEnumerable GenerateFileCache()
        {
            FileEventCache cacheSession = new FileEventCache();

            cacheSession.Add(event1Id, event1Data);
            cacheSession.Add(event2Id, event2Data);
            
            cacheSession.Remove(event1Id);
            
            cacheSession.Dispose();
            
            while (!cacheSession.IsFilesClosed)
                yield return null;
        }
        
        private void OnUnsentEventsLoaded()
        {
            dataLoaded = true;
        }
    }
}