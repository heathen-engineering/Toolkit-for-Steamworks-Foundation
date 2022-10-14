#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="UserStatsStored_t"/> data.
    /// </summary>
    [Serializable]
    public class UserStatsStoredEvent : UnityEvent<UserStatsStored_t>
    { }
}
#endif
