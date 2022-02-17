using System.Collections.Generic;

namespace SberGames.DataPlatform.Core
{
    public interface IEventBuilder
    {
        void IsGetDeviceId(bool isGet);
        void Build(ref EventData eventData, Dictionary<string, string> userParams);
    }
}
