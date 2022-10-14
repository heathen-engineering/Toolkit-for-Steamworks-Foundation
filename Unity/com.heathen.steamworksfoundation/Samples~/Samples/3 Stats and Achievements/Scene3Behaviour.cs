#if HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH) && !DISABLESTEAMWORKS

using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene3Behaviour : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Text winsLabel;
        [SerializeField]
        private UnityEngine.UI.Text achievementLabel;
        [SerializeField]
        private IntStatObject stat;
        [SerializeField]
        private AchievementObject achievement;

        private void Start()
        {
            StatsAndAchievements.Client.EventUserStatsStored.AddListener(HandleStatStored);
            StatsAndAchievements.Client.EventUserAchievementStored.AddListener(HandleStatStored);
            StatsAndAchievements.Client.EventUserStatsReceived.AddListener(HandleStatsReceived);

            UpdateStatsAndAchievements();
        }

        private void HandleStatsReceived(UserStatsReceived_t arg0)
        {
            UpdateStatsAndAchievements();
        }

        private void HandleStatStored(UserAchievementStored_t arg0)
        {
            UpdateStatsAndAchievements();
        }

        private void HandleStatStored(UserStatsStored_t arg0)
        {
            UpdateStatsAndAchievements();
        }

        public void OpenKnowledgeBaseUserData()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/steamworks");
        }

        private void UpdateStatsAndAchievements()
        {
            winsLabel.text = "# Wins: " + stat.Value.ToString();
            if (achievement.IsAchieved)
                achievementLabel.text = "Unlocked";
            else
                achievementLabel.text = "Locked";
        }
    }
}
#endif