#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="UserAchievementStored_t"/> data.
    /// </summary>
    [Serializable]
    public class UserAchievementStoredEvent : UnityEvent<UserAchievementStored_t>
    { }
}
#endif
