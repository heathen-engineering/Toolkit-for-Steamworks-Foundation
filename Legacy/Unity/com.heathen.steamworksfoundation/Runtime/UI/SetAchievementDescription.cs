#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class SetAchievementDescription : MonoBehaviour
    {
        public AchievementObject achievement;
        private TMPro.TextMeshProUGUI description;

        private void Start()
        {
            description = GetComponent<TMPro.TextMeshProUGUI>();

            if (achievement != null)
            {
                if (API.App.Initialized)
                {
                    description.text = achievement.Description;
                }
                else
                    API.App.evtSteamInitialized.AddListener(Refresh);
            }
        }

        public void Refresh()
        {
            description.text = achievement.Description;

            API.App.evtSteamInitialized.RemoveListener(Refresh);
        }
    }
}
#endif