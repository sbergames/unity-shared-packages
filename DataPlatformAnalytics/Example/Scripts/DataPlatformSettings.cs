using UnityEngine;

namespace SberGames.DataPlatform.Example
{
    [CreateAssetMenu(fileName = "DataPlatformSettings", menuName = "SberGames/Analytics/Example/DataPlatformSettingsExample")]
    public class DataPlatformSettings : ScriptableObject
    {
        [SerializeField] private string apiKeyDev = default;
        [SerializeField] private string apiKeyProd = default;
        [SerializeField] private string baseUri = default;

        public string BaseUri => baseUri;
        public string ApiKey
        {
            get
            {
                // ENVIRONMENT_PRODACTION - дефайн устанавливаемый при сборке билда в CI/CD
                #if ENVIRONMENT_PRODACTION
                    return apiKeyProd;
                #else
                    return apiKeyDev;
                #endif
            }
        }
    }
}
