#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class SetAchievementIcon : MonoBehaviour
    {
        public AchievementObject achievement;
        private UnityEngine.UI.RawImage image;

        private void Start()
        {
            image = GetComponent<UnityEngine.UI.RawImage>();

            if (achievement != null)
            {
                achievement.StatusChanged.AddListener(HandleUpdate);

                if (API.App.Initialized)
                {
                    achievement.GetIcon(texture =>
                    {
                        image.texture = texture;
                    });
                }
                else
                    API.App.evtSteamInitialized.AddListener(Refresh);
            }
        }

        private void HandleUpdate(bool arg0)
        {
            Refresh();
        }

        public void Refresh()
        {
            Debug.Log($"{name} Updated");

            achievement.GetIcon(texture =>
            {
                image.texture = texture;
            });

            API.App.evtSteamInitialized.RemoveListener(Refresh);
        }
    }
}
#endif