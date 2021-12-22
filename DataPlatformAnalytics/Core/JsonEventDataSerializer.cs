using Newtonsoft.Json;

namespace SberGames.DataPlatform.Core
{
    public class JsonEventDataSerializer : IEventDataSerializer
    {
        public string Serialize(EventData eventData)
        {
            return JsonConvert.SerializeObject(eventData.data, Formatting.Indented);
        }
    }
}
