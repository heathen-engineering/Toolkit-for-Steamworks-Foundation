#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    public class ToggleEventHelper : MonoBehaviour
    {
        public UnityEvent on;
        public UnityEvent off;

        public void ToggleChanged(bool value)
        {
            if (value)
                on.Invoke();
            else
                off.Invoke();
        }
    }
}
#endif