#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class SetAchievementName : MonoBehaviour
    {
        public AchievementObject achievement;
        private TMPro.TextMeshProUGUI displayName;

        private void Start()
        {
            displayName = GetComponent<TMPro.TextMeshProUGUI>();

            if (achievement != null)
            {
                if (API.App.Initialized)
                {
                    displayName.text = achievement.Name;
                }
                else
                    API.App.evtSteamInitialized.AddListener(Refresh);
            }
        }

        public void Refresh()
        {
            displayName.text = achievement.Name;

            API.App.evtSteamInitialized.RemoveListener(Refresh);
        }
    }
}
#endif