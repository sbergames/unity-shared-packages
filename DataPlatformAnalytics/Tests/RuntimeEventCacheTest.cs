using System.Linq;
using NUnit.Framework;

namespace SberGames.DataPlatform.Core.Tests
{
    public class RuntimeEventCacheTest
    {
        [Test]
        public void CashingTest()
        {
            IEventCache cache = new RuntimeEventCache();

            string event1Id = "sdfh98";
            string event1Data = "{\"someData\": 1}";
            
            string event2Id = "sdmj9y";
            string event2Data = "{\"someData\": 2}";
            
            cache.Add(event1Id, event1Data);
            cache.Add(event2Id, event2Data);
            
            cache.Remove(event1Id);

            var cachedEvents = cache.UnsentEvents().ToList();
            
            Assert.IsTrue(cachedEvents.Count == 1);
            Assert.IsTrue(cachedEvents[0].Key.Equals(event2Id));
            Assert.IsTrue(cachedEvents[0].Value.Equals(event2Data));
            
            cache.Dispose();
        }
    }
}