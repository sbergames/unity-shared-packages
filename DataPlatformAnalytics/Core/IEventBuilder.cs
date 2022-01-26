using System.Collections.Generic;

namespace SberGames.DataPlatform.Core
{
    public interface IEventBuilder
    {
        void Build(ref EventData eventData, Dictionary<string, string> userParams);
    }
}
