#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System.Collections;
using UnityEngine;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// You can test and see what values are set at this URL https://steamcommunity.com/dev/testrichpresence
    /// </summary>
    public class RichPresenceSetter : MonoBehaviour
    {
        public bool setOnEnable = true;
        public bool changeWithAppFocus = false;
        public StringKeyValuePair[] withFocus = new StringKeyValuePair[] { new StringKeyValuePair { key = "steam_display", value = "#Status_AtMainMenu" } };
        public StringKeyValuePair[] withoutFocus;

        private void OnEnable()
        {
            if(API.App.Initialized)
            {
                if (setOnEnable)
                {
                    if (Application.isFocused)
                        Set(withFocus);
                    else
                        Set(withoutFocus);
                }
            }
            else
            {
                API.App.evtSteamInitialized.AddListener(DelayUpdate);
            }

            Application.focusChanged += Application_focusChanged;
        }

        private void DelayUpdate()
        {
            if (setOnEnable)
            {
                if (Application.isFocused)
                    Set(withFocus);
                else
                    Set(withoutFocus);
            }

            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }

        private void OnDisable()
        {
            Application.focusChanged -= Application_focusChanged;
        }

        private void Application_focusChanged(bool focused)
        {
            if (changeWithAppFocus)
            {
                if (focused)
                    Set(withFocus);
                else
                    Set(withoutFocus);
            }
        }

        public void Set(params StringKeyValuePair[] settings)
        {
            foreach(var kvp in settings)
                Friends.Client.SetRichPresence(kvp.key, kvp.value);
        }

        public void Set(string key, string value) => Friends.Client.SetRichPresence(key, value);

        public void Clear() => Friends.Client.ClearRichPresence();
    }
}
#endif