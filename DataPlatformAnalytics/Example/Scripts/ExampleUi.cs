using UnityEngine;

namespace SberGames.DataPlatform.Example
{
    public class ExampleUi : MonoBehaviour
    {
        public void Button1OnClicked()
        {
            DataPlatformAnalyticsProvider.Instance.SendButton1ClickedEvent();
        }
        
        public void Button2OnClicked()
        {
            DataPlatformAnalyticsProvider.Instance.SendButton2ClickedEvent();
        }
    }
}
