#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public class UserStatsReceivedEvent : UnityEvent<UserStatsReceived_t> { }
}
#endif