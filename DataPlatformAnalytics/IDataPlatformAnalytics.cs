
namespace SberGames.DataPlatform
{
    public interface IDataPlatformAnalytics
    {
        void Initialize(string key, string value);
        
        void SetUserProperty(string key, string value);
        
        void SendEvent(EventData data);

        void IsGetDeviceId(bool isGet);
    }
}

