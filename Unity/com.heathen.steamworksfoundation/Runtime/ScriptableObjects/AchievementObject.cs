#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using HeathenEngineering.Events;
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steamworks Achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of an achievement that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/achievement-object")]
    [CreateAssetMenu(menuName = "Steamworks/Achievement Object")]
    public class AchievementObject : ScriptableObject
    {
        public string Id
        {
            get => data;
            set => data = value;
        }

        public string Name => data.Name;

        public string Description => data.Description;

        public bool Hidden => data.Hidden;

        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private AchievementData data;

        /// <summary>
        /// Indicates that this achievment has been unlocked by this user.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public bool IsAchieved
        {
            get => data.IsAchieved;
            set => data.IsAchieved = value;
        }

        public UnityBoolEvent StatusChanged = new UnityBoolEvent();

        /// <summary>
        /// Indicates the time the achievement was unlocked if at all
        /// </summary>
        public DateTime? UnlockTime => data.UnlockTime;

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        public void Unlock() => IsAchieved = true;

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        public void ClearAchievement() => IsAchieved = false;

        /// <summary>
        /// Unlock the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void Unlock(CSteamID user) => data.Unlock(user);

        /// <summary>
        /// Clears the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void ClearAchievement(UserData user) => data.ClearAchievement(user);

        /// <summary>
        /// Gets the achievement status for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool GetAchievementStatus(CSteamID user) => data.GetAchievementStatus(user);

        public (bool unlocked, DateTime unlockTime) GetAchievementAndUnlockTime(UserData user) => data.GetAchievementAndUnlockTime(user);

        public void GetIcon(Action<Texture2D> callback) => API.StatsAndAchievements.Client.GetAchievementIcon(data, callback);

        public void Store() => API.StatsAndAchievements.Client.StoreStats();

        /// <summary>
        /// This will create a ScriptableObject based on this leaderbaord ... in general you should not need this
        /// The AchievementData struct has all the same features of the ScriptableObject but is much lighterweight and
        /// more suitable for creation at runtime.
        /// </summary>
        /// <returns></returns>
        public static AchievementObject CreateScriptableObject(string apiName)
        {
            var newObject = UnityEngine.ScriptableObject.CreateInstance<AchievementObject>();
            newObject.Id = apiName;
            return newObject;
        }
    }
}
#endif