using System.Collections.Generic;
using System.Threading.Tasks;

namespace SberGames.DataPlatform.Core.Net
{
    public interface IEventSender
    {
        void Initialization(string _apiKey, string _host);
        
        Task<SendingResult> Send(string eventData);
        Task<SendingResult> Send(List<string> eventDatas);
    }
}
