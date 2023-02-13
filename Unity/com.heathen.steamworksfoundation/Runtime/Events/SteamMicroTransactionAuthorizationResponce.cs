#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class SteamMicroTransactionAuthorizationResponce : UnityEvent<AppId_t, ulong, bool>
    { }
#elif FACEPUNCH
    [System.Serializable]
    public class SteamMicroTransactionAuthorizationResponce : UnityEvent<AppId, ulong, bool>
    { }
#endif
}
#endif