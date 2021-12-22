using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class DataPlatformUnityObject : MonoBehaviour
    {
        private DataPlatformAnalyticsImpl dataPlatformAnalyticsImpl = null;
        
        public void Initialize(DataPlatformAnalyticsImpl _dataPlatformAnalyticsImpl)
        {
            dataPlatformAnalyticsImpl = _dataPlatformAnalyticsImpl;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                dataPlatformAnalyticsImpl.StartSession();
            }
        }

        private void OnDestroy()
        {
            dataPlatformAnalyticsImpl.Dispose();
        }
    }
}
