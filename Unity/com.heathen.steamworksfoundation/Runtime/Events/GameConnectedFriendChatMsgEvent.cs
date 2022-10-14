#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class GameConnectedFriendChatMsgEvent : UnityEvent<UserData, string, EChatEntryType> { }
#elif FACEPUNCH
    [System.Serializable]
    public class GameConnectedFriendChatMsgEvent : UnityEvent<Friend, string, string> { }
#endif
}
#endif