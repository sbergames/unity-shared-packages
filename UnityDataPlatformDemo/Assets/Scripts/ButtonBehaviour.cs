using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SberGames.DataPlatform.Core;
using SberGames.DataPlatform.Example;

namespace SberGames.DataPlatform.Demo
{
    public class ButtonBehaviour : MonoBehaviour
    {
        public void ClickedEvent()
        {
            DataPlatformAnalyticsWrapper.Instance.SendButtonClickedEvent();
        }

    }
}