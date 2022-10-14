#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class FriendRichPresenceUpdateEvent : UnityEvent<FriendRichPresenceUpdate_t> { }
#elif FACEPUNCH
    public class FriendRichPresenceUpdateEvent : UnityEvent<Friend> { }
#endif
}
#endif