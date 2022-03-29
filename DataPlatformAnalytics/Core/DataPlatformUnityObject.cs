using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class DataPlatformUnityObject : MonoBehaviour
    {
        private static string DataPlatformObjectName = "DataPlatformObject";
        private IDataPlatformAnalyticsImpl dataPlatformAnalyticsImpl = null;
        
        public void Initialize(IDataPlatformAnalyticsImpl _dataPlatformAnalyticsImpl)
        {
            dataPlatformAnalyticsImpl = _dataPlatformAnalyticsImpl;
        }

        private void Awake()
        {
            gameObject.name = DataPlatformObjectName;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            dataPlatformAnalyticsImpl.Dispose();
        }
    }
}
