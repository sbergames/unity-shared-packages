
namespace SberGames.DataPlatform.Core
{
    public interface IEventDataSerializer
    {
        string Serialize(EventData eventData);
    }
}
