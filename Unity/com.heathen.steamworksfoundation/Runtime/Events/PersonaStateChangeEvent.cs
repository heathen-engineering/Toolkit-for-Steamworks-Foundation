#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="PersonaStateChange_t"/> data.
    /// </summary>
#if STEAMWORKSNET
    [Serializable]
    public class PersonaStateChangeEvent : UnityEvent<PersonaStateChange_t>
    { }
#elif FACEPUNCH
    [Serializable]
    public class PersonaStateChangeEvent : UnityEvent<Friend>
    { }
#endif
}
#endif
